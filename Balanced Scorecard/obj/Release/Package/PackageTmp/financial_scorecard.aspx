<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="financial_scorecard.aspx.cs" Inherits="Balanced_Scorecard.financial_scorecard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
      Financial Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><b>Financial Measure</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Financial Measure</h1>
                        </div>
                    </div>
                <!-- /.row -->
            <a id="btnAddMeasure" runat="server" style="margin-top:20px; margin-bottom:-35px"><i class="fa fa fa-plus"></i>&nbsp;&nbsp;Add Group</a>
					<form id="FormExport" runat="server">
                        <div style="text-align:right">
                            <asp:Button CssClass="btn btn-success" ID="ButtonExcel" runat="server" Text="Export to Excel" OnClick="OnClickExportExcel"/>
                            <asp:Button CssClass="btn btn-danger" ID="ButtonPDF" runat="server" Text="Export to PDF" OnClick="OnClickExportPDF"/>
                        </div>
                    </form>
                    <div class="panel panel-primary" id="panel_csc">
                        	<div class="panel-heading"><b>Planning Period:</b>
                                <div class="btn-group" role="group">
                                    <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
                                </div>
                            </div>
                            	<div class="table-responsive">
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
