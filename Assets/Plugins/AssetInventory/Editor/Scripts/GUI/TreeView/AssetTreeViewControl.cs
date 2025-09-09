using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete

namespace AssetInventory
{
    internal sealed class AssetTreeViewControl : TreeViewWithTreeModel<AssetInfo>
    {
        private const float ROW_HEIGHT = 20f;
        private const float TOGGLE_WIDTH = 20f;

        public enum Columns : int
        {
            Name,
            Tags,
            Version,
            Indexed,
            Downloaded,
            ModifiedDate,
            ModifiedDateRelative,
            Publisher,
            Category,
            UnityVersions,
            BIRP,
            URP,
            HDRP,
            License,
            Price,
            ReleaseDate,
            ReleaseDateRelative,
            PurchaseDate,
            PurchaseDateRelative,
            UpdateDate,
            UpdateDateRelative,
            State,
            Source,
            Location,
            Size,
            FileCount,
            Rating,
            RatingCount,
            Update,
            Backup,
            Extract,
            Exclude,
            AICaptions,
            Deprecated,
            Outdated,
            Popularity,
            Materialized,
            ForeignId,
            InternalState
        }

        private readonly List<int> _previousSelection = new List<int>();

        public AssetTreeViewControl(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<AssetInfo> model) : base(state, multiColumnHeader, model)
        {
            rowHeight = ROW_HEIGHT * AI.Config.rowHeightMultiplier;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (ROW_HEIGHT - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = TOGGLE_WIDTH;

            Reload();
        }

        public override void OnGUI(Rect rect)
        {
            // store previous selection to support CTRL-click to toggle selection later
            _previousSelection.Clear();
            _previousSelection.AddRange(state.selectedIDs);

            base.OnGUI(rect);
        }

        protected override void SingleClickedItem(int id)
        {
            // support CTRL-click to toggle selection since tree does not natively support this
            if (Event.current.modifiers != EventModifiers.Control) return;

            if (_previousSelection.Contains(id))
            {
                state.selectedIDs.Remove(id);
                SetSelection(state.selectedIDs, TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        // only build the visible rows, the backend has the full tree information 
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            IList<TreeViewItem> rows = base.BuildRows(root);
            return rows;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            TreeViewItem<AssetInfo> item = (TreeViewItem<AssetInfo>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem<AssetInfo> item, Columns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc. in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            Texture checkmarkIcon = UIStyles.IconContent("Valid", "d_Valid", "|Indexed").image;
            Rect rect = cellRect;

            if (item.Data.AssetId <= 0 && column != Columns.Name) return;

            switch (column)
            {
                case Columns.AICaptions:
                    if (item.Data.UseAI) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.Backup:
                    if (item.Data.Backup) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.BIRP:
                    if (item.Data.BIRPCompatible) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.Category:
                    EditorGUI.LabelField(cellRect, item.Data.GetDisplayCategory());
                    break;

                case Columns.Deprecated:
                    if (item.Data.IsDeprecated) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.Downloaded:
                    if (item.Data.IsDownloaded) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.Exclude:
                    if (item.Data.Exclude) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.Extract:
                    if (item.Data.KeepExtracted) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.FileCount:
                    EditorGUI.LabelField(cellRect, item.Data.FileCount.ToString());
                    break;

                case Columns.ForeignId:
                    if (item.Data.ForeignId > 0) EditorGUI.LabelField(cellRect, item.Data.ForeignId.ToString());
                    break;

                case Columns.HDRP:
                    if (item.Data.HDRPCompatible) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.Indexed:
                    if (item.Data.IsIndexed) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.InternalState:
                    EditorGUI.LabelField(cellRect, item.Data.CurrentState.ToString());
                    break;

                case Columns.License:
                    GUIContent license = new GUIContent(string.IsNullOrWhiteSpace(item.Data.License) ? "-default-" : item.Data.License);
                    EditorGUI.LabelField(cellRect, license);
                    break;

                case Columns.Location:
                    EditorGUI.LabelField(cellRect, item.Data.GetLocation(true));
                    break;

                case Columns.Materialized:
                    if (item.Data.IsMaterialized) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.ModifiedDate:
                    if (item.Data.ModifiedDate != null && item.Data.ModifiedDate.Value.Year > 1)
                    {
                        EditorGUI.LabelField(cellRect, item.Data.ModifiedDate.Value.ToShortDateString());
                    }
                    break;

                case Columns.ModifiedDateRelative:
                    if (item.Data.ModifiedDate != null && item.Data.ModifiedDate.Value.Year > 1)
                    {
                        EditorGUI.LabelField(cellRect, StringUtils.GetRelativeTimeDifference(item.Data.ModifiedDate.Value));
                    }
                    break;

                case Columns.Name:
                    rect.x += GetContentIndent(item);
                    rect.width = TOGGLE_WIDTH - 3;
                    if (item.Data.AssetId > 0)
                    {
                        if (item.Data.PreviewTexture != null)
                        {
                            GUI.DrawTexture(rect, item.Data.PreviewTexture);
                        }
                        else
                        {
                            Texture icon = item.Data.GetFallbackIcon();
                            if (icon != null) GUI.DrawTexture(rect, icon);
                        }
                    }
                    else
                    {
                        Texture folderIcon = UIStyles.IconContent("Folder Icon", "d_Folder Icon").image;
                        GUI.DrawTexture(rect, folderIcon);
                    }

                    // show default icon and label
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                    break;

                case Columns.Outdated:
                    if (item.Data.CurrentSubState == Asset.SubState.Outdated) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.Popularity:
                    EditorGUI.LabelField(cellRect, $"{item.Data.GetRoot().Hotness:N1}");
                    break;

                case Columns.Price:
                    EditorGUI.LabelField(cellRect, item.Data.GetPriceText());
                    break;

                case Columns.Publisher:
                    EditorGUI.LabelField(cellRect, item.Data.GetDisplayPublisher());
                    break;

                case Columns.PurchaseDate:
                    if (item.Data.GetPurchaseDate().Year > 1)
                    {
                        EditorGUI.LabelField(cellRect, item.Data.GetPurchaseDate().ToShortDateString());
                    }
                    break;

                case Columns.PurchaseDateRelative:
                    if (item.Data.GetPurchaseDate().Year > 1)
                    {
                        EditorGUI.LabelField(cellRect, StringUtils.GetRelativeTimeDifference(item.Data.GetPurchaseDate()));
                    }
                    break;

                case Columns.Rating:
                    EditorGUI.LabelField(cellRect, $"{item.Data.AssetRating:N1}");
                    break;

                case Columns.RatingCount:
                    EditorGUI.LabelField(cellRect, item.Data.GetRoot().RatingCount.ToString());
                    break;

                case Columns.ReleaseDate:
                    if (item.Data.FirstRelease.Year > 1)
                    {
                        EditorGUI.LabelField(cellRect, item.Data.FirstRelease.ToShortDateString());
                    }
                    break;

                case Columns.ReleaseDateRelative:
                    if (item.Data.FirstRelease.Year > 1)
                    {
                        EditorGUI.LabelField(cellRect, StringUtils.GetRelativeTimeDifference(item.Data.FirstRelease));
                    }
                    break;

                case Columns.Size:
                    if (item.Data.PackageSize > 0)
                    {
                        EditorGUI.LabelField(cellRect, EditorUtility.FormatBytes(item.Data.PackageSize));
                    }
                    break;

                case Columns.Source:
                    EditorGUI.LabelField(cellRect, StringUtils.CamelCaseToWords(item.Data.AssetSource.ToString()));
                    break;

                case Columns.State:
                    EditorGUI.LabelField(cellRect, item.Data.OfficialState);
                    break;

                case Columns.Tags:
                    if (item.Data.PackageTags.Count > 0)
                    {
                        EditorGUI.LabelField(cellRect, string.Join(", ", item.Data.PackageTags.Select(t => t.Name)));
                    }
                    break;

                case Columns.UnityVersions:
                    EditorGUI.LabelField(cellRect, item.Data.SupportedUnityVersions);
                    break;

                case Columns.Update:
                    if (item.Data.IsUpdateAvailable())
                    {
                        RenderIcon(rect, checkmarkIcon);
                    }
                    break;

                case Columns.UpdateDate:
                    if (item.Data.LastRelease.Year > 1)
                    {
                        EditorGUI.LabelField(cellRect, item.Data.LastRelease.ToShortDateString());
                    }
                    break;

                case Columns.UpdateDateRelative:
                    if (item.Data.LastRelease.Year > 1)
                    {
                        EditorGUI.LabelField(cellRect, StringUtils.GetRelativeTimeDifference(item.Data.LastRelease));
                    }
                    break;

                case Columns.URP:
                    if (item.Data.URPCompatible) RenderIcon(rect, checkmarkIcon);
                    break;

                case Columns.Version:
                    bool updateAvailable = item.Data.IsUpdateAvailable((List<AssetInfo>)TreeModel.GetData());

                    rect.width -= 16;

                    // check if version is missing
                    if ((item.Data.AssetSource == Asset.Source.Archive || item.Data.AssetSource == Asset.Source.CustomPackage) && string.IsNullOrWhiteSpace(item.Data.GetVersion()))
                    {
                        if (AI.ShowAdvanced() && GUI.Button(rect, "enter manually"))
                        {
                            NameUI textUI = new NameUI();
                            textUI.Init("", newVersion => AI.SetVersion(item.Data, newVersion));
                            PopupWindow.Show(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), textUI);
                        }
                    }
                    else
                    {
                        bool isAccurate = item.Data.Origin != null || item.Data.AssetSource == Asset.Source.RegistryPackage;
                        GUIContent version = new GUIContent(item.Data.GetVersion());
                        EditorGUI.LabelField(rect, version, isAccurate ? EditorStyles.boldLabel : EditorStyles.label);

                        if (updateAvailable)
                        {
                            Vector2 size = EditorStyles.label.CalcSize(version);
                            Texture statusIcon = UIStyles.IconContent("preAudioLoopOff", "Update-Available", "|Update Available").image;
                            rect.x += Mathf.Min(size.x, cellRect.width - 16) + 2;
                            rect.y += 1;
                            rect.width = 16;
                            rect.height = 16;
                            Color color = item.Data.AssetSource == Asset.Source.CustomPackage ? Color.gray : Color.white;
                            GUI.DrawTexture(rect, statusIcon, ScaleMode.StretchToFill, true, 0, color, Vector4.zero, Vector4.zero);
                        }
                    }
                    break;

                default:
#if UNITY_2021_2_OR_NEWER
                    int metaId = multiColumnHeader.GetColumn((int)column).userData;
                    MetadataInfo metaInfo = item.Data.PackageMetadata.FirstOrDefault(pm => pm.DefinitionId == metaId);
                    if (metaInfo == null) break;

                    switch (metaInfo.Type)
                    {
                        case MetadataDefinition.DataType.Boolean:
                            if (metaInfo.BoolValue) RenderIcon(rect, checkmarkIcon);
                            break;

                        case MetadataDefinition.DataType.Text:
                        case MetadataDefinition.DataType.BigText:
                        case MetadataDefinition.DataType.SingleSelect:
                            EditorGUI.LabelField(cellRect, metaInfo.StringValue, EditorStyles.wordWrappedLabel);
                            break;

                        case MetadataDefinition.DataType.Number:
                            EditorGUI.LabelField(cellRect, metaInfo.IntValue.ToString());
                            break;

                        case MetadataDefinition.DataType.DecimalNumber:
                            EditorGUI.LabelField(cellRect, $"{metaInfo.FloatValue:N1}");
                            break;

                        case MetadataDefinition.DataType.Url:
                            if (GUI.Button(cellRect, metaInfo.StringValue?.Replace("https://", "").Replace("www.", ""), EditorStyles.linkLabel))
                            {
                                Application.OpenURL(metaInfo.StringValue);
                            }
                            break;

                        case MetadataDefinition.DataType.Date:
                            EditorGUI.LabelField(cellRect, metaInfo.DateTimeValue.ToShortDateString());
                            break;
                    }
#endif
                    break;
            }
        }

        private static void RenderIcon(Rect rect, Texture icon)
        {
            rect.x += rect.width / 2 - 8;
            rect.width = 16;
            rect.height = 16;
            GUI.DrawTexture(rect, icon);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            Dictionary<int, MultiColumnHeaderState.Column> columns = new Dictionary<int, MultiColumnHeaderState.Column>();
            columns[(int)Columns.Name] = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                minWidth = 60,
                autoResize = true,
                allowToggleVisibility = false
            };
            columns[(int)Columns.License] = GetTextColumn("License");
            columns[(int)Columns.Tags] = GetTextColumn("Tags");
            columns[(int)Columns.Version] = GetTextColumn("Version");
            columns[(int)Columns.Indexed] = GetCheckmarkColumn("Indexed");
            columns[(int)Columns.FileCount] = GetTextColumn("Indexed Files");
            columns[(int)Columns.Downloaded] = GetCheckmarkColumn("Downloaded");
            columns[(int)Columns.ModifiedDate] = GetTextColumn("Modified Date");
            columns[(int)Columns.ModifiedDateRelative] = GetTextColumn("Modified Date Rel");
            columns[(int)Columns.Publisher] = GetTextColumn("Publisher");
            columns[(int)Columns.Category] = GetTextColumn("Category");
            columns[(int)Columns.UnityVersions] = GetTextColumn("Unity Versions");
            columns[(int)Columns.BIRP] = GetCheckmarkColumn("BIRP");
            columns[(int)Columns.URP] = GetCheckmarkColumn("URP");
            columns[(int)Columns.HDRP] = GetCheckmarkColumn("HDRP");
            columns[(int)Columns.Price] = GetTextColumn("Price");
            columns[(int)Columns.State] = GetTextColumn("State");
            columns[(int)Columns.Source] = GetTextColumn("Source");
            columns[(int)Columns.Size] = GetTextColumn("Size");
            columns[(int)Columns.Rating] = GetTextColumn("Rating");
            columns[(int)Columns.RatingCount] = GetTextColumn("#Reviews");
            columns[(int)Columns.ReleaseDate] = GetTextColumn("Release Date");
            columns[(int)Columns.ReleaseDateRelative] = GetTextColumn("Release Date Rel");
            columns[(int)Columns.PurchaseDate] = GetTextColumn("Purchase Date");
            columns[(int)Columns.PurchaseDateRelative] = GetTextColumn("Purchase Date Rel");
            columns[(int)Columns.UpdateDate] = GetTextColumn("Update Date");
            columns[(int)Columns.UpdateDateRelative] = GetTextColumn("Update Date Rel");
            columns[(int)Columns.Location] = GetTextColumn("Location");
            columns[(int)Columns.Update] = GetCheckmarkColumn("Update");
            columns[(int)Columns.Backup] = GetCheckmarkColumn("Backup");
            columns[(int)Columns.Extract] = GetCheckmarkColumn("Extract");
            columns[(int)Columns.Exclude] = GetCheckmarkColumn("Exclude");
            columns[(int)Columns.AICaptions] = GetCheckmarkColumn("AI Captions");
            columns[(int)Columns.Popularity] = GetTextColumn("Popularity");
            columns[(int)Columns.Deprecated] = GetCheckmarkColumn("Deprecated");
            columns[(int)Columns.Outdated] = GetCheckmarkColumn("Outdated");
            columns[(int)Columns.Materialized] = GetCheckmarkColumn("Materialized");
            columns[(int)Columns.ForeignId] = GetTextColumn("Foreign Id");
            columns[(int)Columns.InternalState] = GetTextColumn("Processing");

            List<MetadataDefinition> metadataDefs = Metadata.LoadDefinitions();
            if (metadataDefs.Any()) 
            {
                int offset = 100;
                columns[offset] = GetTextColumn(string.Empty);
                foreach (MetadataDefinition mDef in metadataDefs)
                {
                    switch (mDef.Type)
                    {
                        case MetadataDefinition.DataType.Boolean:
                            columns[offset + mDef.Id] = GetCheckmarkColumn(mDef.Name, mDef.Id);
                            break;

                        default:
                            columns[offset + mDef.Id] = GetTextColumn(mDef.Name, 150, mDef.Id);
                            break;
                    }
                }
            }

            MultiColumnHeaderState state = new MultiColumnHeaderState(columns.OrderBy(c => c.Key).Select(c => c.Value).ToArray());
            return state;
        }

        private static MultiColumnHeaderState.Column GetCheckmarkColumn(string name, int idx = 0)
        {
            return new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent(name),
                contextMenuText = name,
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Right,
                canSort = true,
                width = 60,
                minWidth = 30,
                maxWidth = 80,
                autoResize = false,
                allowToggleVisibility = true,
#if UNITY_2021_2_OR_NEWER
                userData = idx
#endif
            };
        }

        private static MultiColumnHeaderState.Column GetTextColumn(string name, int width = 150, int idx = 0)
        {
            return new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent(name),
                contextMenuText = name,
                headerTextAlignment = TextAlignment.Center,
                canSort = true,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Right,
                width = width,
                minWidth = 30,
                autoResize = false,
                allowToggleVisibility = true,
#if UNITY_2021_2_OR_NEWER
                userData = idx
#endif
            };
        }
    }
}