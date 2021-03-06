﻿<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="add_group.aspx.cs" Inherits="Balanced_Scorecard.add_group" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Add Group Financial Measure
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><a id="financial_measures_breadcrumb" runat="server">Financial Measures</a></li>
                              <li><b>Add Group</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Add Group</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form class="form-horizontal" style="margin-top:20px" runat="server">
                      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
                      <div class="form-group">
                        <label for="start_period_label" class="col-md-3 col-sm-5 control-label">Start Period</label>
                        <div class="col-md-6 col-sm-6">
                            <p class="form-control-static" id="LabelStartPeriod" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="end_period_label" class="col-md-3 col-sm-5 control-label">End Period</label>
                        <div class="col-sm-6 col-sm-6">
                            <p class="form-control-static" id="LabelEndPeriod" runat="server"></p>
                        </div>
                      </div>
                      <asp:UpdatePanel ID="UpdatePanelDropDown" runat="server">
                      <ContentTemplate>
                      <div class="form-group">
                        <label class="col-md-3 col-sm-5 control-label">Group Name</label>
                        <div class="col-md-3 col-sm-4">
                            <asp:DropDownList ID="DropDownListGroup" OnSelectedIndexChanged="OnDropdownChanged" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>
                            <p class="error-message" id="check_group_name" runat="server">Your group name already exists</p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxStretch" class="col-md-3 col-sm-5 control-label">Financial Stretch Rating</label>
                              <div class="col-md-3 col-sm-4">
                                  <div class="input-group">
                                      <input type="number" min="0" maxlength="3" step="0.01" class="form-control" id="TextBoxStretch" runat="server" value="0"/>
                				      <div class="input-group-addon">%</div>                      
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxStretchIndividual" class="col-md-3 col-sm-5 control-label">Individual Stretch Rating</label>
                              <div class="col-md-3 col-sm-4">
                                  <div class="input-group">
                                      <input type="number" min="0" maxlength="3" step="0.01" class="form-control" id="TextBoxStretchIndividual" runat="server" value="0"/>
                				      <div class="input-group-addon">%</div>                      
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                        <label class="col-md-3 col-sm-5 control-label">Review</label>
                        <div class="col-md-3 col-sm-4">
                            <asp:DropDownList ID="DropDownListReview" CssClass="form-control" runat="server"></asp:DropDownList>
                        </div>
                      </div>
                      <div class="form-group">
                      	<div class="col-sm-offset-5 col-md-offset-3 col-sm-10">
                          <asp:Label ID="LabelMonthDiff" runat="server" style="color:red; font-weight:bold" Text=""></asp:Label>
                        </div>
                      </div>
                      <div class="form-group">
                      	<div class="col-sm-offset-5 col-md-offset-3 col-sm-8">
                            	<a runat="server" id="cancel_add_new_group" class="btn btn-primary btn-cancel-group"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Add&nbsp;&nbsp;</a>
                              
                              <span style="color:white" class="btn btn-add-group btn-add-group-container add-button" id="SpanAddGroup" runat="server">
                                <i class="fa fa-fw fa-plus"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonSubmit" CssClass="relative-to-btn" OnClick="OnClickSubmit" runat="server" Text="Add" />
                              </span>
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
