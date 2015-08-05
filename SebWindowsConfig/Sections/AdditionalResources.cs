﻿using System;
using System.Windows.Forms;
using SebWindowsClient.ConfigurationUtils;
using ListObj = System.Collections.Generic.List<object>;
using DictObj = System.Collections.Generic.Dictionary<string, object>;

namespace SebWindowsConfig.Sections
{
    public partial class AdditionalResources : UserControl
    {
        public AdditionalResources()
        {
            InitializeComponent();

            SEBSettings.additionalResourcesList = (ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalResources];
            textBoxAdditionalResourcesTitle.Text = "";
            treeViewAdditionalResources.Nodes.Clear();
            foreach (DictObj l0Resource in SEBSettings.additionalResourcesList)
            {
                var l0Node = treeViewAdditionalResources.Nodes.Add(l0Resource[SEBSettings.KeyAdditionalResourcesIdentifier].ToString(), GetDisplayTitle(l0Resource));
                foreach (DictObj l1Resource in (ListObj)l0Resource[SEBSettings.KeyAdditionalResources])
                {
                    var l1Node = l0Node.Nodes.Add(l1Resource[SEBSettings.KeyAdditionalResourcesIdentifier].ToString(), GetDisplayTitle(l1Resource));
                    foreach (DictObj l2Resource in (ListObj)l1Resource[SEBSettings.KeyAdditionalResources])
                    {
                        l1Node.Nodes.Add(l2Resource[SEBSettings.KeyAdditionalResourcesIdentifier].ToString(), GetDisplayTitle(l2Resource));
                    }
                }
            }
        }

        private string GetDisplayTitle(DictObj resource)
        {
            return String.Format("{0}{1}", resource[SEBSettings.KeyAdditionalResourcesTitle],
                (bool) resource[SEBSettings.KeyAdditionalResourcesActive] ? "" : " (inactive)");
        }

        private void buttonAdditionalResourcesAdd_Click(object sender, EventArgs e)
        {
            // Get the process list
            SEBSettings.additionalResourcesList = (ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalResources];

            int newIndex = treeViewAdditionalResources.Nodes.Count;
            SEBSettings.additionalResourcesList.Insert(newIndex, CreateNewResource(newIndex.ToString()));

            treeViewAdditionalResources.SelectedNode = treeViewAdditionalResources.Nodes.Add(newIndex.ToString(), "New Resource");
            treeViewAdditionalResources.Focus();
        }

        private DictObj CreateNewResource(string identifier)
        {
            DictObj resourceData = new DictObj();
            resourceData[SEBSettings.KeyAdditionalResources] = new ListObj();
            resourceData[SEBSettings.KeyAdditionalResourcesActive] = true;
            resourceData[SEBSettings.KeyAdditionalResourcesAutoOpen] = false;
            resourceData[SEBSettings.KeyAdditionalResourcesIdentifier] = identifier;
            resourceData[SEBSettings.KeyAdditionalResourcesResourceIcons] = new ListObj();
            resourceData[SEBSettings.KeyAdditionalResourcesTitle] = "New Resource";
            resourceData[SEBSettings.KeyAdditionalResourcesUrl] = "";
            return resourceData;
        }

        private void buttonAdditionalResourcesAddSubResource_Click(object sender, EventArgs e)
        {
            var node = treeViewAdditionalResources.SelectedNode;
            if (node == null)
                MessageBox.Show("No node selected");
            if (node.Level == 2)
                MessageBox.Show("Maximum 3 levels");

            var selectedResource = GetSelectedResource();
            ListObj resourceList = (ListObj)selectedResource[SEBSettings.KeyAdditionalResources];

            var newIndex = node.Nodes.Count;
            if (node.Level == 0)
            {
                resourceList.Add(CreateNewResource(node.Index + "." + newIndex));
                treeViewAdditionalResources.SelectedNode = treeViewAdditionalResources.SelectedNode.Nodes.Add(node.Index + "." + newIndex, "New Resource");
            }
            if (node.Level == 1)
            {
                resourceList.Add(CreateNewResource(node.Parent.Index + "." + node.Index + "." + newIndex));
                treeViewAdditionalResources.SelectedNode = treeViewAdditionalResources.SelectedNode.Nodes.Add(node.Parent.Index + "." + node.Index + "." + newIndex, "New Resource");
            }
            treeViewAdditionalResources.Focus();
        }

        private void treeViewAdditionalResources_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DictObj selectedResource = GetSelectedResource();
            textBoxAdditionalResourcesTitle.Text = (string)selectedResource[SEBSettings.KeyAdditionalResourcesTitle];
            checkBoxAdditionalResourceActive.Checked = (bool)selectedResource[SEBSettings.KeyAdditionalResourcesActive];
            textBoxAdditionalResourceUrl.Text = (string)selectedResource[SEBSettings.KeyAdditionalResourcesUrl];
        }

        private DictObj GetSelectedResource()
        {
            var node = treeViewAdditionalResources.SelectedNode;

            if (node.Level == 0)
            {
                return (DictObj)SEBSettings.additionalResourcesList[node.Index];
            }
            else if (node.Level == 1)
            {
                DictObj rootResource = (DictObj)SEBSettings.additionalResourcesList[node.Parent.Index];
                ListObj level1List = (ListObj)rootResource[SEBSettings.KeyAdditionalResources];
                return (DictObj)level1List[node.Index];
            }
            else if (node.Level == 2)
            {
                DictObj rootResource = (DictObj)SEBSettings.additionalResourcesList[treeViewAdditionalResources.SelectedNode.Parent.Parent.Index];
                ListObj level1List = (ListObj)rootResource[SEBSettings.KeyAdditionalResources];
                DictObj level1Resource = (DictObj)level1List[treeViewAdditionalResources.SelectedNode.Parent.Index];
                ListObj level2List = (ListObj)level1Resource[SEBSettings.KeyAdditionalResources];
                return (DictObj)level2List[node.Index];
            }
            return null;
        }

        private void UpdateAdditionalResourceIdentifiers()
        {
            foreach (TreeNode l0Node in treeViewAdditionalResources.Nodes)
            {
                DictObj l0resource = (DictObj)SEBSettings.additionalResourcesList[l0Node.Index];
                l0resource[SEBSettings.KeyAdditionalResourcesIdentifier] = l0Node.Index.ToString();
                foreach (TreeNode l1Node in l0Node.Nodes)
                {
                    ListObj l1resources = (ListObj)l0resource[SEBSettings.KeyAdditionalResources];
                    DictObj l1resource = (DictObj) l1resources[l1Node.Index];
                    l1resource[SEBSettings.KeyAdditionalResourcesIdentifier] = l0Node.Index + "." + l1Node.Index;
                    foreach (TreeNode l2Node in l1Node.Nodes)
                    {
                        ListObj l2resources = (ListObj)l1resource[SEBSettings.KeyAdditionalResources];
                        DictObj l2resource = (DictObj)l2resources[l2Node.Index];
                        l2resource[SEBSettings.KeyAdditionalResourcesIdentifier] = l0Node.Index + "." + l1Node.Index + "." + l2Node.Index;
                    }
                }
            }
        }

        private void buttonAdditionalResourcesMoveUp_Click(object sender, EventArgs e)
        {
            var nodeToMove = treeViewAdditionalResources.SelectedNode;
            if (nodeToMove.Index == 0)
                return;

            var oldIndex = nodeToMove.Index;

            var parent = treeViewAdditionalResources.SelectedNode.Parent;
            if (parent == null)
            {
                var nodeToMoveDown = treeViewAdditionalResources.Nodes[oldIndex - 1];
                treeViewAdditionalResources.Nodes.RemoveAt(oldIndex - 1);
                treeViewAdditionalResources.Nodes.Insert(oldIndex, nodeToMoveDown);
                DictObj resourceToMoveDown = (DictObj)SEBSettings.additionalResourcesList[oldIndex - 1];
                SEBSettings.additionalResourcesList.RemoveAt(oldIndex -1);
                SEBSettings.additionalResourcesList.Insert(oldIndex, resourceToMoveDown);
            }
            else
            {
                var nodeToMoveDown = parent.Nodes[oldIndex - 1];
                parent.Nodes.RemoveAt(oldIndex - 1);
                parent.Nodes.Insert(oldIndex, nodeToMoveDown);
                DictObj parentResource = new DictObj();
                if (parent.Level == 0)
                {
                    parentResource = (DictObj)SEBSettings.additionalResourcesList[parent.Index];
                }
                if (parent.Level == 1)
                {
                    DictObj l0Resource = (DictObj)SEBSettings.additionalResourcesList[parent.Parent.Index];
                    ListObj l0ResourcesList = (ListObj)l0Resource[SEBSettings.KeyAdditionalResources];
                    parentResource = (DictObj)l0ResourcesList[parent.Index];
                }
                ListObj parentResourceList = (ListObj) parentResource[SEBSettings.KeyAdditionalResources];
                DictObj resourceToMoveDown = (DictObj)parentResourceList[oldIndex - 1];
                parentResourceList.RemoveAt(oldIndex -1);
                parentResourceList.Insert(oldIndex, resourceToMoveDown);
            }

            UpdateAdditionalResourceIdentifiers();
        }

        private void buttonAdditionalResourcesMoveDown_Click(object sender, EventArgs e)
        {
            var nodeToMove = treeViewAdditionalResources.SelectedNode;

            var oldIndex = nodeToMove.Index;

            var parent = treeViewAdditionalResources.SelectedNode.Parent;
            if (parent == null)
            {
                if (nodeToMove.Index == treeViewAdditionalResources.Nodes.Count -1)
                    return;
                var nodeToMoveUp = treeViewAdditionalResources.Nodes[oldIndex + 1];
                treeViewAdditionalResources.Nodes.RemoveAt(oldIndex + 1);
                treeViewAdditionalResources.Nodes.Insert(oldIndex, nodeToMoveUp);
                DictObj resourceToMoveUp = (DictObj) SEBSettings.additionalResourcesList[oldIndex + 1];
                SEBSettings.additionalResourcesList.RemoveAt(oldIndex + 1);
                SEBSettings.additionalResourcesList.Insert(oldIndex, resourceToMoveUp);
            }
            else
            {
                if (nodeToMove.Index == parent.Nodes.Count -1 )
                    return;
                var nodeToMoveUp = parent.Nodes[nodeToMove.Index + 1];
                parent.Nodes.RemoveAt(nodeToMove.Index + 1);
                parent.Nodes.Insert(oldIndex, nodeToMoveUp);
                DictObj parentResource = new DictObj();
                if (parent.Level == 0)
                {
                    parentResource = (DictObj)SEBSettings.additionalResourcesList[parent.Index];
                }
                if (parent.Level == 1)
                {
                    DictObj l0Resource = (DictObj)SEBSettings.additionalResourcesList[parent.Parent.Index];
                    ListObj l0ResourcesList = (ListObj)l0Resource[SEBSettings.KeyAdditionalResources];
                    parentResource = (DictObj)l0ResourcesList[parent.Index];
                }
                ListObj parentResourceList = (ListObj)parentResource[SEBSettings.KeyAdditionalResources];
                DictObj resourceToMoveDown = (DictObj)parentResourceList[oldIndex + 1];
                parentResourceList.RemoveAt(oldIndex + 1);
                parentResourceList.Insert(oldIndex, resourceToMoveDown);
            }

            UpdateAdditionalResourceIdentifiers();
        }

        private void buttonadditionalResourcesRemove_Click(object sender, EventArgs e)
        {
            var node = treeViewAdditionalResources.SelectedNode;

            if (node.Level == 0)
            {
                SEBSettings.additionalResourcesList.RemoveAt(node.Index);
            }
            else if (node.Level == 1)
            {
                DictObj rootResource = (DictObj)SEBSettings.additionalResourcesList[node.Parent.Index];
                ListObj level1List = (ListObj)rootResource[SEBSettings.KeyAdditionalResources];
                level1List.RemoveAt(node.Index);
            }
            else if (node.Level == 2)
            {
                DictObj rootResource = (DictObj)SEBSettings.additionalResourcesList[treeViewAdditionalResources.SelectedNode.Parent.Parent.Index];
                ListObj level1List = (ListObj)rootResource[SEBSettings.KeyAdditionalResources];
                DictObj level1Resource = (DictObj)level1List[treeViewAdditionalResources.SelectedNode.Parent.Index];
                ListObj level2List = (ListObj)level1Resource[SEBSettings.KeyAdditionalResources];
                level2List.RemoveAt(node.Index);
            }
            node.Remove();

            UpdateAdditionalResourceIdentifiers();
        }

        private void checkBoxAdditionalResourceActive_CheckedChanged(object sender, EventArgs e)
        {
            DictObj selectedResource = GetSelectedResource();
            selectedResource[SEBSettings.KeyAdditionalResourcesActive] = checkBoxAdditionalResourceActive.Checked;

            treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);
        }

        private void textBoxAdditionalResourcesTitle_TextChanged(object sender, EventArgs e)
        {
            DictObj selectedResource = GetSelectedResource();
            selectedResource[SEBSettings.KeyAdditionalResourcesTitle] = textBoxAdditionalResourcesTitle.Text;

            treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);
        }

        private void textBoxAdditionalResourceUrl_TextChanged(object sender, EventArgs e)
        {
            DictObj selectedResource = GetSelectedResource();
            selectedResource[SEBSettings.KeyAdditionalResourcesUrl] = textBoxAdditionalResourceUrl.Text;
        }
    }
}