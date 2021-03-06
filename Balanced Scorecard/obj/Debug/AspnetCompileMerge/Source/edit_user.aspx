﻿<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="edit_user.aspx.cs" Inherits="Balanced_Scorecard.edit_user" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Edit Scorecard User | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><a id="scorecard_user_breadcrumb" runat="server">Scorecard Users</a></li>
                              <li><b>Edit <asp:Label ID="LabelUserName_Breadcrumb" runat="server" Text="User Name"></asp:Label> (<asp:Label ID="LabelUserNIK_Breadcrumb" runat="server" Text="User NIK"></asp:Label>)</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-user"></i>&nbsp;Edit <asp:Label ID="LabelUserName_Title" runat="server" Text="User Name"></asp:Label> (<asp:Label ID="LabelUserNIK_Title" runat="server" Text="User NIK"></asp:Label>)</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form id="Form1" class="form-horizontal" style="margin-top:20px" runat="server" autocomplete="off">
                      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
                      <asp:UpdatePanel ID="UpdatePanelSuperior" runat="server"> 
                      <ContentTemplate>
                      <div class="form-group">
                        <label class="col-sm-3 col-md-2 control-label">Period</label>
                        <div class="col-md-5 col-sm-6">
                            <p class="form-control-static" runat="server" id="ScorecardPeriod"></p>
                        </div>
                      </div>
                      <div id="TextBoxColor" runat="server">
                        <label for="TextBoxNIK" class="col-sm-3 col-md-2 control-label">NIK</label>
                        <div class="col-md-3 col-sm-5">
                            <asp:TextBox ID="TextBoxNIK" ReadOnly="true" required="required" CssClass="form-control" AutoPostBack="true" placeholder="Your NIK" runat="server"></asp:TextBox>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxName" class="col-sm-3 col-md-2 control-label">Name</label>
                        <div class="col-md-4 col-sm-6">
                          <input type="text" class="form-control" runat="server" readonly="readonly" id="TextBoxName" placeholder="Your Name" required="required"/>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxOrganization" class="col-sm-3 col-md-2 control-label">Organization</label>
                        <div class="col-md-4 col-sm-6">
                          <input type="text" class="form-control" runat="server" readonly="readonly" id="TextBoxOrganization" placeholder="Your Organization" required="required"/>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxJobTitle" class="col-sm-3 col-md-2 control-label">Job Title</label>
                        <div class="col-md-4 col-sm-6">
                          <input type="text" class="form-control" runat="server" readonly="readonly" id="TextBoxJobTitle" placeholder="Your Job Title" required="required"/>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxGrade" class="col-sm-3 col-md-2 control-label">Grade</label>
                        <div class="col-md-4 col-sm-6">
                          <input type="text" class="form-control" runat="server" readonly="readonly" id="TextBoxGrade" placeholder="Your Grade" required="required"/>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxJobLevelName" class="col-sm-3 col-md-2 control-label">Job Level Name</label>
                        <div class="col-md-4 col-sm-6">
                          <input type="text" class="form-control" runat="server" readonly="readonly" id="TextBoxJobLevelName" placeholder="Your Job Level Name" required="required"/>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxJobTitle" class="col-sm-3 col-md-2 control-label">E-Mail</label>
                        <div class="col-md-4 col-sm-6">
                          <input type="email" class="form-control" runat="server" readonly="readonly" id="TextBoxEmail" placeholder="Your E-Mail" required="required"/>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxGroup" class="col-sm-3 col-md-2 control-label">Scorecard Group</label>
                        <div class="col-md-4 col-sm-6">
                          <input type="text" class="form-control" runat="server" readonly="readonly" id="TextBoxGroup" placeholder="Your Scorecard Group" required="required"/>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxAdditional" class="col-sm-3 col-md-2 control-label">Additional Group</label>
                        <div class="col-md-4 col-sm-6">
                          <input type="text" class="form-control" runat="server" readonly="readonly" id="TextBoxAdditional" placeholder="Your Additional Group" required="required"/>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="active_status" class="col-sm-3 col-md-2 control-label">Role</label>
                        <div class="col-md-2 col-sm-4">
                            <asp:DropDownList CssClass="form-control" ID="DropDownListRole" runat="server"></asp:DropDownList>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxSuperior" class="col-sm-3 col-md-2 control-label">Superior NIK</label>
                        <div class="col-md-3 col-sm-5">
                            <asp:TextBox ID="TextBoxSuperior" runat="server" OnTextChanged="OnSuperiorChanged" AutoPostBack="true" Placeholder="Superior NIK" CssClass="form-control"></asp:TextBox>
                            <asp:Label ID="LabelSuperiorName" runat="server" Visible="false"></asp:Label>
                        </div>
                      </div>
                      </ContentTemplate>
                      </asp:UpdatePanel>
                      <div class="form-group">
                        <label for="TextBoxPassword" class="col-sm-3 col-md-2 control-label">Password</label>
                        <div class="col-md-3 col-sm-5">
                            <asp:TextBox ID="TextBoxPassword" runat="server" required="required" Placeholder="Password" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </div>
                      </div>
                      <asp:UpdatePanel ID="UpdatePanelConfirmation" runat="server"> 
                      <ContentTemplate>
                      <div id="TextBoxColorConfirmation" runat="server">
                        <label for="TextBoxConfirm" class="col-sm-3 col-md-2 control-label">Confirm Password</label>
                        <div class="col-md-3 col-sm-5">
                            <asp:TextBox Placeholder="Re-Type Your Password" ID="TextBoxConfirmation" CssClass="form-control" TextMode="Password" OnTextChanged="TextBoxConfirmation_TextChanged" AutoPostBack="true" runat="server"></asp:TextBox>
                            <p class="error-message" id="check_password_confirmation" runat="server">Password and Confirm Password is not match!</p>
                        </div>
                      </div>
                      </ContentTemplate>
                      </asp:UpdatePanel>
                      <div class="form-group">
                        <label for="active_status" class="col-sm-3 col-md-2 control-label">Active</label>
                        <div class="col-md-2 col-sm-4">
                            <asp:DropDownList CssClass="form-control" ID="DropDownListStatus" runat="server"></asp:DropDownList>
                        </div>
                      </div>
                      <asp:UpdatePanel ID="UpdatePanelEditButton" runat="server"> 
                      <ContentTemplate>
                      <div class="form-group">
                      	<div class="col-md-offset-2 col-sm-offset-3 col-sm-10">
                      		<p>
                            	<a class="btn btn-primary btn-cancel-add-detail" id="cancel_edit" runat="server"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Edit&nbsp;&nbsp;</a>
                                <span id="SpanAddUser" style="color:white; margin-left:-25px" class="btn btn-add-more-finance btn-add-more-container add-button" runat="server">
                                <i class="fa fa-fw fa-pencil-square-o"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonEditUser" OnClick="OnClickEditUser" CssClass="relative-to-btn-more" runat="server" Text="Edit User" />
                                </span>
                            </p>
                        </div>
                      </div>
                      </ContentTemplate>
                      </asp:UpdatePanel>
                    </form>
                <!-- End of FORM -->
            </div>
            <!-- /.container-fluid -->

        </div>
        <!-- /#page-wrapper -->s
</asp:Content>
