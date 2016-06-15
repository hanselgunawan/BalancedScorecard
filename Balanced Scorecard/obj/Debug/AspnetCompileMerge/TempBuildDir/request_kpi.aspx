<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="request_kpi.aspx.cs" Inherits="Balanced_Scorecard.request_kpi" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Requested KPI | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>My Requests</li>
                              <li><b>KPIs</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-list"></i>&nbsp;Add/Change/Delete Requests for Individual KPIs</h1>
                        </div>
                    </div>
                <!-- /.row -->
                <form class="form-horizontal" id="PeriodForm" style="margin-top:20px" runat="server">
                    <asp:ScriptManager ID="ScriptManagerDashboard" runat="server"></asp:ScriptManager>
                    <asp:UpdatePanel ID="UpdatePanelSetPeriod" runat="server">
                        <ContentTemplate>
                            <div class="form-group">
                            <label for="start_date" class="col-sm-2 col-md-1 control-label">Period</label>
                                <div class="col-sm-5 col-md-3">
                                     <asp:PlaceHolder ID="PlaceHolderPeriod" runat="server"></asp:PlaceHolder>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </form>
					<div class="panel panel-primary" id="panel_csc">
                        	<div class="panel-heading"><p class="csc_p">List of Add/Change/Delete Requests for Individual KPIs</p></div>
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
