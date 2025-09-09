using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetInventory
{
    public sealed class PreviewPipeline : AssetImporter
    {
        public async Task<int> RecreateScheduledPreviews(List<AssetInfo> assets, List<AssetInfo> allAssets)
        {
            string assetFilter = GetAssetFilter(assets);
            string query = $@"
                SELECT *, af.Id as Id 
                FROM AssetFile af
                INNER JOIN Asset ON Asset.Id = af.AssetId 
                WHERE Asset.Exclude = false 
                AND (af.PreviewState=? OR af.PreviewState=?) 
                {assetFilter}
                AND af.Id = (
                    SELECT af2.Id 
                    FROM AssetFile af2 
                    WHERE af2.AssetId = af.AssetId 
                    AND (af2.PreviewState=? OR af2.PreviewState=?)
                    ORDER BY af2.Id 
                    LIMIT 1
                )
                ORDER BY Asset.Id";
            List<AssetInfo> files = DBAdapter.DB.Query<AssetInfo>(query, AssetFile.PreviewOptions.Redo, AssetFile.PreviewOptions.RedoMissing, AssetFile.PreviewOptions.Redo, AssetFile.PreviewOptions.RedoMissing).ToList();

            return await RecreatePreviews(files, true, allAssets);
        }

        public static string GetAssetFilter(List<AssetInfo> assets, string fieldName = "Asset.Id")
        {
            string assetFilter = "";
            if (assets != null && assets.Count > 0)
            {
                assetFilter = $"and {fieldName} in (";
                foreach (AssetInfo asset in assets)
                {
                    assetFilter += asset.AssetId + ",";
                }

                assetFilter = assetFilter.Substring(0, assetFilter.Length - 1) + ")";
            }
            return assetFilter;
        }

        public async Task<int> RecreatePreviews(List<AssetInfo> files, bool packageMode, List<AssetInfo> allAssets, bool autoRemoveCache = true, Action<PreviewRequest> onDone = null)
        {
            int created = 0;

            // check if previewable at all, do here once in non-package mode as that can reduce the resultset dramatically 
            if (!packageMode) files.RemoveAll(item => !PreviewManager.IsPreviewable(item.FileName, true, item));

            List<IGrouping<int, AssetInfo>> assetGroups = files
                .GroupBy(info => info.AssetId)
                .OrderByDescending(group => group.Key)
                .ToList();

            MainCount = assetGroups.Count;
            foreach (IGrouping<int, AssetInfo> grouping in assetGroups)
            {
                if (CancellationRequested) break;

                List<AssetInfo> infos;
                if (packageMode)
                {
                    // in package mode, files are not loaded yet to save memory 
                    string query = @"
                        SELECT *, af.Id as Id 
                        FROM AssetFile af
                        INNER JOIN Asset ON Asset.Id = af.AssetId 
                        WHERE Asset.Id = ?
                        AND Asset.Exclude = false
                        AND (af.PreviewState=? OR af.PreviewState=?)";
                    infos = DBAdapter.DB.Query<AssetInfo>(query, grouping.Key, AssetFile.PreviewOptions.Redo, AssetFile.PreviewOptions.RedoMissing).ToList();

                    // check if previewable at all
                    infos.RemoveAll(item => !PreviewManager.IsPreviewable(item.FileName, true, item));
                    if (infos.Count == 0) continue;

                    AI.ResolveParents(infos, allAssets);
                }
                else
                {
                    infos = grouping.ToList();
                }
                AssetInfo info = infos.First();

                // check state
                Asset asset = info.ToAsset();
                string tempPath = AI.GetMaterializedAssetPath(asset);
                bool wasCached = Directory.Exists(tempPath);
                bool wasDownloaded = false;

                MainProgress++;
                SetProgress(asset.DisplayName, MainProgress);

                // download on demand
                if (!info.IsDownloaded && !info.IsMaterialized)
                {
                    if (!AI.Config.downloadPackagesForPreviews || !CanDownload(info))
                    {
                        Debug.Log($"Could not recreate previews for '{asset}' since the package is not downloaded.");
                        continue;
                    }

                    // ensure package is downloaded
                    IEnumerator downloader = DownloadAsset(info);
                    while (downloader.MoveNext())
                    {
                        if (CancellationRequested) break;
                        await Task.Yield();
                    }
                    if (CancellationRequested) break;
                    if (!info.IsDownloaded)
                    {
                        Debug.Log($"Could not recreate preview for '{asset}' since the package could not be downloaded.");
                        continue;
                    }
                    wasDownloaded = true;
                }

                // perform actual preview recreation
                created += await RecreatePackagePreviews(infos, onDone);

                // clean up again
                if (!wasCached && autoRemoveCache) RemoveWorkFolder(asset, tempPath);
                if (wasDownloaded)
                {
                    // remove downloaded package if it was not cached
                    IEnumerator remover = RemoveDownload(asset);
                    while (remover.MoveNext())
                    {
                        if (CancellationRequested) break;
                        await Task.Yield();
                    }
                }
            }

            return created;
        }

        private async Task<int> RecreatePackagePreviews(List<AssetInfo> files, Action<PreviewRequest> onDone = null)
        {
            int created = 0;

            UnityPreviewGenerator.Init(files.Count);

            SubCount = files.Count;
            SubProgress = 0;

            // Process files in batches for parallel processing
            for (int i = 0; i < files.Count; i += AI.Config.parallelPreviewBatchSize)
            {
                if (CancellationRequested) break;

                // Get current batch
                int batchSize = Math.Min(AI.Config.parallelPreviewBatchSize, files.Count - i);
                List<AssetInfo> batch = files.GetRange(i, batchSize);

                // Process batch in parallel (but still on main thread)
                List<Task> batchTasks = new List<Task>();
                foreach (AssetInfo info in batch)
                {
                    if (CancellationRequested) break;

                    SubProgress++;
                    CurrentSub = info.FileName;

                    // Create task for this preview
                    Task previewTask = ProcessPreviewAsync(info, () => created++, onDone);
                    batchTasks.Add(previewTask);
                }

                // Wait for all previews in this batch to complete
                if (batchTasks.Count > 0)
                {
                    await Task.WhenAll(batchTasks);
                }

                // Let the editor breathe between batches
                if (AI.Config.parallelPreviewBatchSize > 1)
                {
                    await Task.Yield();
                }
            }
            SubProgress = SubCount; // ensure 100% progress

            CurrentSub = "Exporting Previews...";
            await UnityPreviewGenerator.ExportPreviews();

            CurrentSub = "Cleaning Up...";
            UnityPreviewGenerator.CleanUp();

            return created;
        }

        private async Task ProcessPreviewAsync(AssetInfo info, Action onCreated, Action<PreviewRequest> onDone)
        {
            await AI.Cooldown.Do();
            await PreviewManager.Create(info, null, onCreated, onDone);
        }

        public async Task<int> RestorePreviews(List<AssetInfo> assets, List<AssetInfo> allAssets)
        {
            int restored = 0;

            string previewPath = AI.GetPreviewFolder();
            string assetFilter = GetAssetFilter(assets);
            string query = $"select *, AssetFile.Id as Id from AssetFile inner join Asset on Asset.Id = AssetFile.AssetId where Asset.Exclude = false and (Asset.AssetSource = ? or Asset.AssetSource = ?) and AssetFile.PreviewState != ? {assetFilter} order by Asset.Id";
            List<AssetInfo> files = DBAdapter.DB.Query<AssetInfo>(query, Asset.Source.AssetStorePackage, Asset.Source.CustomPackage, AssetFile.PreviewOptions.Provided).ToList();
            AI.ResolveParents(files, allAssets);

            MainCount = files.Count;
            foreach (AssetInfo info in files)
            {
                MainProgress++;
                SetProgress(info.FileName, MainProgress);

                if (CancellationRequested) break;
                await AI.Cooldown.Do();
                if (MainProgress % 50 == 0) await Task.Yield(); // let editor breath 

                if (!info.IsDownloaded && !info.IsMaterialized) continue;

                string previewFile = info.GetPreviewFile(previewPath);
                string sourcePath = await AI.EnsureMaterializedAsset(info);
                if (sourcePath == null)
                {
                    if (info.PreviewState != AssetFile.PreviewOptions.NotApplicable)
                    {
                        info.PreviewState = AssetFile.PreviewOptions.None;
                        DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", info.PreviewState, info.Id);
                    }
                    continue;
                }

                string originalPreviewFile = PreviewManager.DerivePreviewFile(sourcePath);
                if (!File.Exists(originalPreviewFile))
                {
                    if (info.PreviewState != AssetFile.PreviewOptions.NotApplicable)
                    {
                        info.PreviewState = AssetFile.PreviewOptions.None;
                        DBAdapter.DB.Execute("update AssetFile set PreviewState=? where Id=?", info.PreviewState, info.Id);
                    }
                    continue;
                }

                File.Copy(originalPreviewFile, previewFile, true);
                info.PreviewState = AssetFile.PreviewOptions.Provided;
                info.Hue = -1f;
                DBAdapter.DB.Execute("update AssetFile set PreviewState=?, Hue=? where Id=?", info.PreviewState, info.Hue, info.Id);

                restored++;
            }

            return restored;
        }
    }
}