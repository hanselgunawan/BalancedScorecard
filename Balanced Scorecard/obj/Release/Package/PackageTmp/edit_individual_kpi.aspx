<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="edit_individual_kpi.aspx.cs" Inherits="Balanced_Scorecard.edit_individual_kpi" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Edit Individual KPI | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">

            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><a id="individual_scorecard_breadcrumb" runat="server">Individual Scorecard</a></li>
                              <li><b><asp:Label ID="LabelUpdateEdit" runat="server" Text="Label"></asp:Label> KPI (<asp:Label ID="LabelBreadcrumb" runat="server" Text="Header"></asp:Label>)</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;<asp:Label ID="LabelUpdateEdit2" runat="server" Text="Label"></asp:Label> KPI (<asp:Label ID="LabelTitle" runat="server" Text="Header"></asp:Label>)</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <!-- FORM -->
                    <form id="Form1" class="form-horizontal" style="margin-top:20px" runat="server">
                      <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                      <asp:UpdatePanel ID="UpdatePanelEditKPI" runat="server">
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
                        <label for="TextBoxKPI" class="col-md-2 col-sm-3 control-label">Specific Obj.</label>
                        <div class="col-md-2 col-sm-3">
                            <asp:DropDownList ID="DropDownListSpecific" AutoPostBack="true" OnSelectedIndexChanged="OnSelectSpecific" CssClass="form-control" runat="server"></asp:DropDownList>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxKPI" class="col-md-2 col-sm-3 control-label">KPI</label>
                        <div class="col-md-6 col-sm-8">
                          <asp:TextBox ID="TextBoxKPI" OnTextChanged="OnKPIChanged" AutoPostBack="true" runat="server" CssClass="form-control" placeholder="Your Key Performance Indicator" required="required"></asp:TextBox>
                          <p class="error-message" id="check_KPI_name_error_message" runat="server">Your KPI already exists</p>
                        </div>
                      </div>
                      <div class="form-group">
                        <label for="TextareaObjective" class="col-md-2 col-sm-3 control-label">Objective</label>
                            <div class="col-md-5 col-sm-7">
                              <textarea id="TextareaObjective" required="required" runat="server" class="form-control" rows="3" placeholder="Type Some Objective"></textarea>
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
                        <label for="TextBoxResult" class="col-md-2 col-sm-3 control-label">Result</label>
                            <div class="col-md-3 col-sm-5">
                           		<asp:TextBox ID="TextBoxResult" CssClass="form-control" OnTextChanged="OnResultChanged" AutoPostBack="true" required="required" runat="server"></asp:TextBox>
                                <p class="error-message" id="month_name_result" runat="server"></p>               	
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectMeasure" class="col-md-2 col-sm-3 control-label">Measure By</label>
                            <div class="col-md-3 col-sm-5">
                                <asp:DropDownList ID="DropDownListMeasurement" OnSelectedIndexChanged="OnSelectMeasureBy" AutoPostBack="true" CssClass="form-control" runat="server"></asp:DropDownList>	
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="SelectFormula" class="col-md-2 col-sm-3 control-label">Formula</label><!-- Hard-Coded -->
                            <div class="col-md-3 col-sm-5">
                                <asp:DropDownList ID="DropDownFormula" CssClass="form-control" runat="server"></asp:DropDownList>	
                            </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxRating" class="col-md-2 col-sm-3 control-label">Rating</label>
                              <div class="col-md-3 col-sm-5">
                                  <div class="input-group">
                                  <input type="number" min="0" step="0.01" class="form-control" id="TextBoxRating" runat="server" value="0" required="required" />
                				  <div class="input-group-addon">%</div>                      
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxWeight" class="col-md-2 col-sm-3 control-label">Weight</label>
                              <div class="col-md-3 col-sm-5">
                                  <div class="input-group">
                                  <input type="number" min="0" max="100" step="0.01" class="form-control" id="TextBoxWeight" runat="server" value="0" required="required"/>
                                  <div class="input-group-addon">%</div>
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                        <label for="TextBoxScore" class="col-md-2 col-sm-3 control-label">Score</label>
                              <div class="col-md-3 col-sm-5">
                                  <div class="input-group">
                                  <input type="number" min="0" max="100" step="0.01" class="form-control" id="TextBoxScore" runat="server" value="0" required="required"/>
                                  <div class="input-group-addon">%</div>
                                  </div>
                              </div>
                      </div>
                      <div class="form-group">
                      	<div class="col-md-offset-2 col-sm-offset-3 col-sm-10">
                            	<a runat="server" id="cancel_add_new_KPI" class="btn btn-primary btn-cancel-group"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Cancel Edit&nbsp;&nbsp;</a>
                              <span style="color:white; width:100px" class="btn btn-add-group btn-add-group-container add-button" id="SpanEditKPI" runat="server">
                                <i class="fa fa-fw fa-pencil-square-o"></i>
                                  <asp:Button BackColor="Transparent" style="width:100px" BorderColor="Transparent" ID="ButtonAddKPI" CssClass="relative-to-btn" OnClick="OnClickEditKPI" Text="Edit" runat="server" />
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
