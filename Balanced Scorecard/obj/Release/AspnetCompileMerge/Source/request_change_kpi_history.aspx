<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="request_change_kpi_history.aspx.cs" Inherits="Balanced_Scorecard.request_change_kpi_history" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    KPIs CR History | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Requests History</li>
                              <li><b>KPI Requests History</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-list"></i>&nbsp;KPI Requests History</h1>
                        </div>
                    </div>
                <!-- /.row -->
                <form class="form-horizontal" id="PeriodForm" style="margin-top:20px" runat="server">
                    <asp:ScriptManager ID="ScriptManagerDashboard" runat="server"></asp:ScriptManager>
                    <asp:UpdatePanel ID="UpdatePanelSetPeriod" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="row" style="margin-bottom:20px">
                                <div class="col-xs-1 col-sm-1 col-md-1" style="margin-top:5px; text-align:right">
                                    <b>Period</b>
                                </div>
                                <div class="col-xs-4 col-sm-4 col-md-3">
                                    <asp:PlaceHolder ID="PlaceHolderPeriod" runat="server"></asp:PlaceHolder>
                                </div>
                                <div class="col-xs-2 col-sm-2 col-md-3">
                                </div>
                                <div class="col-xs-5 col-sm-5 col-md-5">
                                    <asp:Button CssClass="btn btn-success form-control" ID="ButtonExcel" runat="server" Text="Recap All KPI Add/Change/Delete Request" OnClick="OnClickExportExcel"/>
                                </div>
                            </div>
                            <div class="row">
                              <div class="col-xs-3 col-md-4">
                              </div>
                              <div class="col-xs-4 col-md-3">
                                  <asp:DropDownList ID="DropDownListSearchBy" CssClass="form-control" runat="server"></asp:DropDownList>
                              </div>
                              <div class="col-xs-5 col-md-5">
                                  <div class="input-group">
                                      <div class="input-group-addon" style="background-color:transparent"><i class="fa fa fa-search"></i></div>
                                      <input type="text" id="TextBoxSearch" runat="server" class="form-control search-query" /> 
                                      <span class="input-group-btn">
                                          <asp:Button ID="ButtonSearch" OnClick="OnClickSearch" runat="server" CssClass="btn btn-search" Text="Search" />
                                      </span>
                                  </div>
                              </div>
                            </div>
                        </ContentTemplate>
                            <Triggers>
                                <asp:PostBackTrigger ControlID="ButtonExcel"></asp:PostBackTrigger>
                            </Triggers>
                    </asp:UpdatePanel>
                </form>
					<div class="panel panel-primary" id="panel_csc">
                        	<div class="panel-heading"><p class="csc_p">History of KPI Change Requests</p></div>
                            	<div class="table-responsive" id="ApprovalTable" runat="server">
                                    <table class="table table-bordered">
                                        <asp:PlaceHolder ID="PlaceHolderApproval" runat="server"></asp:PlaceHolder>
                                    </table>
                            	</div>
                        </div>
                    <div class="my-paging">
                        <asp:PlaceHolder ID="PlaceHolderPaging" runat="server"></asp:PlaceHolder>       
                   </div>
            </div>
            <!-- /.container-fluid -->
        </div>
        <!-- /#page-wrapper -->
</asp:Content>
