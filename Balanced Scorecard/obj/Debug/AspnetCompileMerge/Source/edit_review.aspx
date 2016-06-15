<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="edit_review.aspx.cs" Inherits="Balanced_Scorecard.edit_review" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Edit Review
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><a id="set_review_breadcrumb" runat="server">Set Review Month</a></li>
                              <li><b>Edit Review Month</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Edit Review Month</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form id="Form1" class="form-horizontal" style="margin-top:20px" runat="server">
                      <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                      <div class="form-group">
                        <label for="start_period_label" class="col-md-3 col-sm-5 control-label">Review Name</label>
                        <div class="col-md-6 col-sm-6">
                            <p class="form-control-static" id="LabelReviewName" runat="server"></p>
                        </div>
                      </div>
                      <asp:UpdatePanel ID="UpdatePanelReviewMonth" runat="server">
                      <ContentTemplate>
                      <div class="form-group">
                        <label for="TextBoxGroup" class="col-md-3 col-sm-5 control-label">Review Month</label>
                        <div class="col-md-3 col-sm-4">
                            <asp:DropDownList ID="DropDownListMonth" EnableViewState="true" OnSelectedIndexChanged="OnMonthChanged" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>
                            <p class="error-message" id="check_review_month" runat="server">Your review month already passed</p>
                            <p class="error-message" id="check_review_month_exist" runat="server">Your month already exist</p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxGroup" class="col-md-3 col-sm-5 control-label">Reviewable</label>
                        <div class="col-md-2 col-sm-4">
                            <asp:DropDownList ID="DropDownListReviewable" OnSelectedIndexChanged="OnReviewableChanged" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>
                            Month(s) Before
                            <p class="error-message" id="review_error_message" runat="server">Cannot review from last year</p>
                        </div>
                      </div>
                          <div class="form-group">
                        <label for="TextBoxGroup" class="col-md-3 col-sm-5 control-label">Review Status</label>
                        <div class="col-md-3 col-sm-4">
                            <asp:DropDownList ID="DropDownListStatus" OnSelectedIndexChanged="OnStatusChanged" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>
                            <p class="error-message" id="check_review_status" runat="server">There is an active review month</p>
                        </div>
                      </div>
                      <div class="form-group">
                      	<div class="col-md-offset-3 col-sm-offset-5 col-sm-8">
                            	<a runat="server" id="cancel_edit_review_month" class="btn btn-primary btn-cancel-group"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Edit&nbsp;&nbsp;</a>
                              
                              <span style="color:white" id="SpanEditGroup" runat="server" class="btn btn-success btn-add-group btn-add-group-container edit-button">
                                <i class="fa fa-fw fa-pencil-square-o"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonEdit" OnClick="OnClickEdit" CssClass="relative-to-edit-btn" runat="server" Text="Edit" />
                              </span>
                              <p class="error-message" id="check_month_pass" runat="server">Selected month already passed</p>
                        </div>
                      </ContentTemplate>
                      </asp:UpdatePanel>
                      </div>
                    </form>
                <!-- End of FORM -->
            </div>
            <!-- /.container-fluid -->
        </div>
        <!-- /#page-wrapper -->
</asp:Content>
