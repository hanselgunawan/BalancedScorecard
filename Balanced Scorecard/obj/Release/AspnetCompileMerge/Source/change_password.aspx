<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="change_password.aspx.cs" Inherits="Balanced_Scorecard.change_password" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Change Password | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><b>Change Password</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-user"></i>&nbsp;Change Password</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form id="Form1" class="form-horizontal" style="margin-top:10px" runat="server">
                      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
                      <div class="form-group">
                        <label for="TextBoxPassword" class="col-sm-3 col-md-2 control-label">Current Password</label>
                        <div class="col-md-3 col-sm-5">
                            <asp:TextBox ID="TextBoxPassword" runat="server" required="required" Placeholder="Current Password" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </div>
                      </div>
                      <asp:UpdatePanel ID="UpdatePanelNewPassword" UpdateMode="Conditional" runat="server"> 
                      <ContentTemplate>
                      <div id="TextBoxColorNewPassword" runat="server">
                        <label for="TextBoxPassword" class="col-sm-3 col-md-2 control-label">New Password</label>
                        <div class="col-md-3 col-sm-5">
                            <asp:TextBox ID="TextBoxNewPassword" runat="server" OnTextChanged="TextBoxPassword_TextChanged" AutoPostBack="true" Placeholder="New Password" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            <p class="error-message" id="check_new_password" runat="server">Type New Password</p>
                        </div>
                      </div>
                      </ContentTemplate>
                      </asp:UpdatePanel>
                      <asp:UpdatePanel ID="UpdatePanelConfirmation" UpdateMode="Conditional" runat="server"> 
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
                      <asp:UpdatePanel ID="UpdatePanelChangeButton" UpdateMode="Conditional" runat="server"> 
                      <ContentTemplate>
                      <div class="form-group">
                      	<div class="col-md-offset-2 col-sm-offset-3 col-md-10 col-sm-12">
                      		<p>
                            	<a class="btn btn-primary btn-cancel-add-detail" id="cancel_change" runat="server"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Change&nbsp;&nbsp;</a>
                                <span id="SpanAddUser" style="color:white; margin-left:-25px" class="btn btn-add-more-finance btn-change-pass-container add-button" runat="server">
                                <i class="fa fa-fw fa-key"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonChangePass" OnClick="OnClickChangePassword" CssClass="relative-to-btn-change" runat="server" Text="Change Password" />
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
        <!-- /#page-wrapper -->
</asp:Content>
