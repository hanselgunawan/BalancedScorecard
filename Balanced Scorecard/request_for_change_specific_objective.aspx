<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="request_for_change_specific_objective.aspx.cs" Inherits="Balanced_Scorecard.request_for_change_specific_objective" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Request For Change Specific Objective | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">

            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><a id="individual_scorecard_breadcrumb" runat="server">Individual Scorecard</a></li>
                              <li><b>Request For Change/Delete <asp:Label ID="LabelBreadcrumb" runat="server" Text="breadcrumb"></asp:Label>'s Specific Objective</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Request For Change/Delete <asp:Label ID="LabelTitle" runat="server" Text="title"></asp:Label>'s Specific Objective</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form id="Form1" class="form-horizontal" style="margin-top:20px" runat="server">
                      <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
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
                        <label for="LabelReview" class="col-md-2 col-sm-3 control-label">KPI</label>
                        <div class="col-sm-3">
                          <p class="form-control-static" id="LabelKPI" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxSpecificObjective" class="col-md-2 col-sm-3 control-label">Specific Obj.</label>
                        <div class="col-md-6 col-sm-8">
                          <asp:TextBox ID="TextBoxSpecificObjective" MaxLength="200" OnTextChanged="OnSpecificChanged" AutoPostBack="true" CssClass="form-control" runat="server" placeholder="Your Specific Objective" required="required"></asp:TextBox>
                          <p class="error-message" id="specific_objective_error_message" runat="server">Your specific objective already exists</p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxTarget" class="col-md-2 col-sm-3 control-label">Target</label>
                        <div class="col-md-3 col-sm-5">
                          		<asp:TextBox ID="TextBoxTarget" CssClass="form-control" OnTextChanged="OnTargetChanged" AutoPostBack="true" required="required" runat="server"></asp:TextBox>
                                <p class="error-message" id="month_name_target" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectMeasure" class="col-md-2 col-sm-3 control-label">Measured By</label>
                            <div class="col-md-3 col-sm-5">
                                <asp:DropDownList ID="DropDownListMeasurement" OnSelectedIndexChanged="OnSelectMeasureBy" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>		
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectFormula" class="col-md-2 col-sm-3 control-label">Formula</label>
                            <div class="col-md-4 col-sm-6">
                                <asp:DropDownList ID="DropDownFormula" CssClass="form-control" runat="server"></asp:DropDownList>
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxResult" class="col-md-2 col-sm-3 control-label">Result</label>
                            <div class="col-md-3 col-sm-5">
                           		<asp:TextBox ID="TextBoxResult" CssClass="form-control" OnTextChanged="OnResultChanged" AutoPostBack="true" required="required" runat="server"></asp:TextBox>
                                <p class="error-message" id="month_name_result" runat="server"></p>                            	
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxRating" class="col-md-2 col-sm-3 control-label">Rating</label>
                              <div class="col-md-3 col-sm-5">
                                  <div class="input-group">
                                  <input type="number" step="0.01" class="form-control" id="TextBoxRating" value="0" runat="server" required="required" />
                				  <div class="input-group-addon">%</div>                      
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxSpecificObjective" class="col-md-2 col-sm-3 control-label">Reason</label>
                        <div class="col-md-6 col-sm-8">
                          <textarea id="TextareaReason" maxlength="250" required="required" runat="server" class="form-control" rows="3" placeholder="Why you request for change this Specific Objective?"></textarea>
                        </div>
                      </div>
                      <div class="form-group">
                      	<div class="col-md-3 col-sm-4 col-xs-4">
                        	<a class="btn btn-primary btn-cancel-add-detail" id="cancel_edit_specific" runat="server"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Request&nbsp;&nbsp;</a>
                        </div>
                        <div class="col-md-3 col-sm-4 col-xs-4">
                            <span style="color:white; width:210px; margin-left:-20px" id="SpanEditSpecific" runat="server" class="btn btn-success btn-add-group btn-add-group-container edit-button">
                            <i class="fa fa-fw fa-pencil-square-o"></i>
                                <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonEditDetail" style="width:210px" CssClass="relative-to-edit-btn" OnClick="OnClickRequestSpecific" runat="server" Text="Change Spec. Objective" />
                            </span>
                        </div>
                        <div class="col-md-3 col-sm-4 col-xs-4">
                            <span style="color:white; width:200px" id="SpanDeleteSpecific" runat="server" class="btn btn-success btn-delete-group btn-add-group-container edit-button">
                            <i class="fa fa-fw fa-trash-o"></i>
                                <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonDeleteDetail" style="width:200px" CssClass="relative-to-edit-btn" OnClick="OnClickDeleteSpecific" runat="server" Text="Delete Spec. Objective" />
                            </span>
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
