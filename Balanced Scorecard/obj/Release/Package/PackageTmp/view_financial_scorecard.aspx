<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="view_financial_scorecard.aspx.cs" Inherits="Balanced_Scorecard.view_financial_scorecard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    View Financial Scorecard | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><a id="link_breadcrumb" runat="server">Scorecard Users</a></li>
                              <li><b>View Financial Measure</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Financial Measure</h1>
                        </div>
                    </div>
                <!-- /.row -->
                <a id="btnBackToScorecardUsers" runat="server" class="btn btn-primary" style="margin-top:20px"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Back to Scorecard Users</a>
                <form id="FormExport" runat="server">
                    <div style="text-align:right; margin-top:-35px">
                      <asp:Button CssClass="btn btn-danger" ID="ButtonPDF" runat="server" Text="Export to PDF" OnClick="OnClickExportPDF"/>
                      <asp:Button CssClass="btn btn-success" ID="ButtonExcel" runat="server" Text="Export to Excel" OnClick="OnClickExportExcel"/>
                    </div>
                </form>
                <p style="margin-top:30px; margin-bottom:-10px"><b>Period:</b> <asp:Label ID="LabelPeriod" runat="server" Text="Label"></asp:Label></p>
                    <div class="panel panel-primary" id="panel_csc">
                        	<div class="panel-heading"><b>Financial Measures</b>
                            </div>
                            	<div class="table-responsive" style="overflow-x:visible">
                                    <table class="table table-bordered">
                                        <asp:PlaceHolder ID="PlaceHolder2" runat="server"></asp:PlaceHolder>
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
