﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProviderConfiguration.ascx.cs" Inherits="DotNetNuke.Modules.HtmlEditorManager.Views.ProviderConfiguration" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>

<div class="html-editor-manager">
    <div class="current-provider">
        <h4>Current Provider:</h4>
        <span><%#this.Model.SelectedEditorProvider %></span>
    </div>
    <div class="change-provider">
        <asp:DropDownList ID="ProvidersDropDownList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ProvidersDropDownList_SelectedIndexChanged" DataSource="<%#this.Model.EditorProviders %>" SelectedValue="<%#this.Model.SelectedEditorProvider %>" />
        <asp:Button ID="SaveButton" runat="server" class="dnnPrimaryAction" OnClick="SaveButton_Click" Text="Change" Enabled="<%#this.Model.CanSave %>" />
    </div>
    <div class="dnnClear"></div>
    <asp:PlaceHolder runat="server" ID="EditorPanel" />
</div>