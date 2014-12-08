#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

namespace DotNetNuke.Modules.HtmlEditorManager.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.UI;
    using System.Xml;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.HtmlEditorManager.ViewModels;
    using DotNetNuke.Modules.HtmlEditorManager.Views;
    using DotNetNuke.Web.Mvp;

    /// <summary>
    /// Presenter for Provider Configuration
    /// </summary>
    public class ProviderConfigurationPresenter : ModulePresenter<IProviderConfigurationView, ProviderConfigurationViewModel>
    {
        /// <summary>The HTML editor node</summary>
        private const string HtmlEditorNode = "/configuration/dotnetnuke/htmlEditor";

        /// <summary>The dot net nuke document</summary>
        private XmlDocument dotnetNukeDocument;

        /// <summary>Initializes a new instance of the <see cref="ProviderConfigurationPresenter" /> class.</summary>
        /// <param name="view">the interface provider view.</param>
        public ProviderConfigurationPresenter(IProviderConfigurationView view)
            : base(view)
        {
            this.View.Initialize += this.View_Initialize;
            this.View.SaveEditorChoice += this.View_SaveEditorChoice;
            this.View.EditorChanged += this.View_EditorChanged;
        }

        /// <summary>Gets the DNN configuration.</summary>
        /// <value>The DNN configuration.</value>
        protected XmlDocument DNNConfiguration
        {
            get
            {
                if (this.dotnetNukeDocument == null)
                {
                    UserInfo currentUser = UserController.Instance.GetCurrentUserInfo();
                    if (currentUser != null && currentUser.IsSuperUser)
                    {
                        this.dotnetNukeDocument = Config.Load();
                    }
                }

                return this.dotnetNukeDocument;
            }
        }

        /// <summary>Handles the Initialize event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void View_Initialize(object sender, EventArgs e)
        {
            this.View.Model.EditorProviders = this.GetAvailableEditors();
            this.View.Model.SelectedEditorProvider = this.GetSelectedEditor();
            this.View.Editor.Controls.Add(this.LoadCurrentEditor(this.View.Model.SelectedEditorProvider));
        }

        /// <summary>Handles the Editor was changed event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EditorEventArgs"/> instance containing the event data.</param>
        private void View_EditorChanged(object sender, EditorEventArgs e)
        {
            this.Response.Redirect(TestableGlobals.Instance.NavigateURL(this.TabId, string.Empty, "provider=" + e.Editor), true);
        }

        /// <summary>Handles the SaveEditorChoice event of the View control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EditorEventArgs"/> instance containing the event data.</param>
        private void View_SaveEditorChoice(object sender, EditorEventArgs e)
        {
            this.SaveEditorInConfiguration(e.Editor);
            this.RedirectToCurrentPage();
        }

        /// <summary>Loads the current editor.</summary>
        /// <param name="editorName">Name of the editor.</param>
        /// <returns>The editor based on the current editor settings in the web configuration</returns>
        private Control LoadCurrentEditor(string editorName)
        {
            XmlDocument dnnConfiguration = this.DNNConfiguration;
            XmlNode htmlProviderNode = GetHtmlEditorProviderNode(dnnConfiguration);
            XmlNode currentProvider = htmlProviderNode.SelectSingleNode("providers/add[@name='" + editorName + "']");

            var settingsAttribute = currentProvider.Attributes["settingsControlPath"];
            if (settingsAttribute != null)
            {
                try
                {
                    var controlPath = settingsAttribute.Value;
                    var control = (PortalModuleBase)((TemplateControl)this.View).LoadControl(controlPath);
                    control.ModuleConfiguration = this.ModuleContext.Configuration;
                    control.ID = Path.GetFileNameWithoutExtension(controlPath);
                    this.View.Model.CanSave = true;
                    return control;
                }
                catch (Exception)
                {
                    this.View.Model.CanSave = false;
                }
            }

            // Display a nice message to the user that this provider is not supported.
            return ((TemplateControl)this.View).LoadControl("~/DesktopModules/Admin/HtmlEditorManager/Controls/InvalidConfiguration.ascx");
        }

        /// <summary>Saves the editor in configuration.</summary>
        /// <param name="name">The name.</param>
        private void SaveEditorInConfiguration(string name)
        {
            var providerNode = GetHtmlEditorProviderNode(this.DNNConfiguration);
            if (providerNode != null)
            {
                providerNode.Attributes["defaultProvider"].Value = name;
                Config.Save(this.DNNConfiguration);
            }
        }

        /// <summary>Gets the HTML provider node.</summary>
        /// <param name="dnnConfiguration">The DNN configuration.</param>
        /// <returns>The XmlNode for the htmlEditor provider.</returns>
        private static XmlNode GetHtmlEditorProviderNode(XmlDocument dnnConfiguration)
        {
            if (dnnConfiguration != null && dnnConfiguration.DocumentElement != null)
            {
                return dnnConfiguration.DocumentElement.SelectSingleNode(HtmlEditorNode);
            }

            return null;
        }

        /// <summary>Gets the provider list.</summary>
        /// <returns>A list of the installed providers</returns>
        private IEnumerable<string> GetAvailableEditors()
        {
            if (this.DNNConfiguration == null || this.DNNConfiguration.DocumentElement == null)
            {
                yield break;
            }
            
            var editorNodes = this.DNNConfiguration.DocumentElement.SelectNodes(HtmlEditorNode + "/providers/add");
            if (editorNodes == null)
            {
                yield break;
            }

            int i = 0;
            while (i < editorNodes.Count)
            {
                var node = editorNodes[i];
                if (node.Attributes["name"] != null)
                {
                    yield return node.Attributes["name"].Value;
                }

                i = i + 1;
            }
        }

        /// <summary>Gets the selected editor.</summary>
        /// <returns>The currently configured editor</returns>
        private string GetSelectedEditor()
        {
            if (this.View.Model.EditorProviders.Contains(this.Request.QueryString["provider"], StringComparer.OrdinalIgnoreCase))
            {
                return this.Request.QueryString["provider"];
            }

            if (this.DNNConfiguration != null && this.DNNConfiguration.DocumentElement != null)
            {
                XmlNode editorProviderNode = this.DNNConfiguration.DocumentElement.SelectSingleNode(HtmlEditorNode);
                return editorProviderNode.Attributes["defaultProvider"].Value;
            }

            return string.Empty;
        }
    }
}