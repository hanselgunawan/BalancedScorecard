<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="add_specific_objective.aspx.cs" Inherits="Balanced_Scorecard.add_specific_objective" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Add Specific Objective | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">

            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><a id="individual_scorecard_breadcrumb" runat="server">Individual Scorecard</a></li>
                              <li><b><asp:Label ID="LabelRequestAdd" runat="server" Text="Label"></asp:Label> <asp:Label ID="LabelBreadcrumb" runat="server" Text="breadcrumb"></asp:Label>'s Specific Objective</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;<asp:Label ID="LabelRequestAdd2" runat="server" Text="Label"></asp:Label> <asp:Label ID="LabelTitle" runat="server" Text="title"></asp:Label>'s Specific Objective</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form class="form-horizontal" style="margin-top:20px" runat="server">
                      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
                      <asp:UpdatePanel ID="UpdatePanelAddSpecObjective" runat="server">
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
                          <p class="form-control-static" id="LabelStretch" runat="server">150%</p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="LabelReview" class="col-md-2 col-sm-3 control-label">Review</label>
                        <div class="col-sm-3">
                          <p class="form-control-static" id="LabelReview" runat="server">Yearly</p>
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
                        <div class="col-md-6 col-sm-7">
                          <asp:TextBox ID="TextBoxSpecificObjective" OnTextChanged="OnSpecificChanged" AutoPostBack="true" runat="server" placeholder="Your Specific Objective" required="required" CssClass="form-control"></asp:TextBox>
                          <p class="error-message" id="specific_objective_error_message" runat="server">Your specific objective already exist</p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxTarget" class="col-md-2 col-sm-3 control-label">Target</label>
                        <div class="col-md-3 col-sm-5">
                          		<asp:TextBox ID="TextBoxTarget" CssClass="form-control" OnTextChanged="OnTargetChanged" AutoPostBack="true" required="required" runat="server"></asp:TextBox>
                                <p class="error-message" id="check_target_value" runat="server"></p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxResult" class="col-md-2 col-sm-3 control-label">Result</label>
                            <div class="col-md-3 col-sm-5">
                           		<input type="number" min="0" class="form-control" id="TextBoxResult" value="0" runat="server" required="required"/>                            	
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectMeasure" class="col-md-2 col-sm-3 control-label">Measure By</label>
                            <div class="col-md-3 col-sm-5">
                                <asp:DropDownList ID="DropDownListMeasurement" OnSelectedIndexChanged="OnSelectMeasureBy" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>		
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectFormula" class="col-md-2 col-sm-3 control-label">Formula</label>
                            <div class="col-md-3 col-sm-5">
                                <asp:DropDownList ID="DropDownFormula" CssClass="form-control" runat="server"></asp:DropDownList>
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxRating" class="col-md-2 col-sm-3 control-label">Rating</label>
                              <div class="col-md-3 col-sm-5">
                                  <div class="input-group">
                                  <input type="number" min="0" max="150" step="0.01" class="form-control" id="TextBoxRating" value="0" runat="server" required="required" />
                				  <div class="input-group-addon">%</div>                      
                                  </div>
                              </div>
                      </div>
                      <div class="form-group" id="ReasonTextBox" style="visibility:hidden;position:absolute" runat="server">
                        <label for="TextareaObjective" class="col-md-2 col-sm-3 control-label">Reason</label>
                            <div class="col-md-5 col-sm-7">
                              <textarea id="TextAreaReason" runat="server" class="form-control" rows="3" placeholder="Why you want to add this new KPI?"></textarea>
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
                                <i class="fa fa-fw fa-thumbs-o-up"></i>
                                  <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="Button1" CssClass="relative-to-btn" OnClick="OnClickDone" runat="server" Text="Done" />
                                </span>
                            </p>
                        </div>
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
