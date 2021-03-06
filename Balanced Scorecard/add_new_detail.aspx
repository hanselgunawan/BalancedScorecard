﻿<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="add_new_detail.aspx.cs" Inherits="Balanced_Scorecard.add_new_detail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
        Add Detail | Financial Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><a id="financial_measure_breadcrumb" runat="server">Financial Measures</a></li>
                              <li><b>Add New Measure (<asp:Label ID="LabelBreadcrumb" runat="server" Text="Breadcrumb"></asp:Label>)</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Add New Measure (<asp:Label ID="LabelTitle" runat="server" Text="Title"></asp:Label>)</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form class="form-horizontal" runat="server" style="margin-top:20px">
                      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
                      <asp:UpdatePanel ID="UpdatePanelEditGroupDetail" runat="server">
                      <ContentTemplate>
                      <div class="form-group">
                        <label for="LabelStartPeriod" class="col-md-2 col-sm-3 control-label">Start Period</label>
                        <div class="col-sm-3">
                          <p class="form-control-static" id="LabelStartPeriod" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="LabelEndPeriod" class="col-md-2 col-sm-3 control-label">End Period</label>
                        <div class="col-sm-3">
                          <p class="form-control-static" id="LabelEndPeriod" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="LabelGroup" class="col-md-2 col-sm-3 control-label">Group</label>
                        <div class="col-sm-3">
                          <p class="form-control-static" id="LabelGroup" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="LabelStretch" class="col-md-2 col-sm-3 control-label">Stretch Rating</label>
                        <div class="col-sm-3">
                          <p class="form-control-static" id="LabelStretch" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="LabelReview" class="col-md-2 col-sm-3 control-label">Review</label>
                        <div class="col-sm-3">
                          <p class="form-control-static" id="LabelReview" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxFinancialMeasure" class="col-md-2 col-sm-3 control-label">Financial Measure</label>
                        <div class="col-md-6 col-sm-8">
                          <asp:TextBox ID="TextBoxFinancialMeasure" MaxLength="200" OnTextChanged="OnFinancialMeasureChanged" AutoPostBack="true" CssClass="form-control" placeholder="Your Financial Measure" required="required" runat="server"></asp:TextBox>
                          <p class="error-message" id="check_financial_measure" runat="server">Your financial measure already exists</p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxFinancialType" class="col-md-2 col-sm-3 control-label">Financial Type</label>
                        <div class="col-md-3 col-sm-5">
                            <asp:DropDownList ID="DropDownFinancialType" AutoPostBack="true" OnSelectedIndexChanged="OnSelectType" CssClass="form-control" runat="server"></asp:DropDownList>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxTarget" class="col-md-2 col-sm-3 control-label">Target</label>
                        <div class="col-md-3 col-sm-5">
                                <asp:TextBox ID="TextBoxTarget" MaxLength="15" CssClass="form-control" OnTextChanged="OnTargetChanged" AutoPostBack="true" required="required" runat="server"></asp:TextBox>
                                <p class="error-message" id="check_target_value" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectMeasure" class="col-md-2 col-sm-3 control-label">Measured By</label>
                            <div class="col-md-3 col-sm-5">
                                <asp:DropDownList CssClass="form-control" OnSelectedIndexChanged="OnSelectMeasureBy" AutoPostBack="true" ID="DropDownMeasurement" runat="server"></asp:DropDownList>	
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectFormula" class="col-md-2 col-sm-3 control-label">Formula</label><!-- Hard-Coded -->
                            <div class="col-md-4 col-sm-6">
                                <asp:DropDownList ID="DropDownFormula" CssClass="form-control" runat="server"></asp:DropDownList>	
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxResult" class="col-md-2 col-sm-3 control-label">Result</label>
                            <div class="col-md-3 col-sm-5">
                           		<input type="number" runat="server" min="0" maxlength="15" class="form-control" required="required" id="TextBoxResult" value="0"/>                            	
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxRating" class="col-md-2 col-sm-3 control-label">Rating</label>
                              <div class="col-md-3 col-sm-5">
                                  <div class="input-group">
                                  <input type="number" runat="server" min="0" required="required" step="0.01" class="form-control" id="TextBoxRating" value="0"/>
                				  <div class="input-group-addon">%</div>                      
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxWeight" class="col-md-2 col-sm-3 control-label">Weight</label><!-- Kalau Pilih Share, ga bisa diisi! -->
                              <div class="col-md-3 col-sm-5">
                                  <div class="input-group">
                                  <input type="number" runat="server" min="0" required="required" max="100" step="0.01" class="form-control" id="TextBoxWeight" value="0"/>
                                  <div class="input-group-addon">%</div>
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxScore" class="col-md-2 col-sm-3 control-label">Score</label>
                              <div class="col-md-3 col-sm-5">
                                  <div class="input-group">
                                  <input type="number" runat="server" min="0" required="required" step="0.01" class="form-control" id="TextBoxScore" value="0"/>
                                  <div class="input-group-addon">%</div>
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                        <label for="TextareaRemarks" class="col-md-2 col-sm-3 control-label">Remarks</label>
                            <div class="col-md-5 col-sm-7">
                              <textarea id="TextareaRemarks" maxlength="250" runat="server" class="form-control" rows="3" placeholder="Type Some Remarks"></textarea>
                            </div>
                      </div>
                      <div class="form-group">
                      	<div class="col-md-offset-2 col-sm-offset-3 col-sm-10">
                      		<p>
                            	<a class="btn btn-primary btn-cancel-add-detail" id="cancel_add" runat="server"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Add&nbsp;&nbsp;</a>

                                <span id="SpanAddMore" style="color:white; margin-right:15px; margin-left:-20px" class="btn btn-add-more-finance btn-add-more-container add-button" runat="server">
                                <i class="fa fa-fw fa-plus"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonAddMore" OnClick="OnClickAddMore" CssClass="relative-to-btn-more" runat="server" Text="Add More" />
                                </span>

                                <span id="SpanDone" style="color:white" runat="server" class="btn btn-add-group btn-add-group-container add-button">
                                <i class="fa fa-fw fa-plus"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="Button1" CssClass="relative-to-btn" OnClick="OnClickAddDetail" runat="server" Text="Add" />
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
