<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="individual_scorecard.aspx.cs" Inherits="Balanced_Scorecard.individual_scorecard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Individual Scorecard | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">

            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><b>Individual Scorecard</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Individual Scorecard</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <a id="btnNewKPI" runat="server" style="margin-top:20px; margin-bottom:-35px"><i class="fa fa fa-plus"></i>&nbsp;&nbsp;<asp:Label ID="LabelAddRequest" runat="server" Text="Add New KPI"></asp:Label></a>
                <form id="FormExport" runat="server">
                    <div style="text-align:right">
                      <asp:Button CssClass="btn btn-success" ID="ButtonExcel" runat="server" Text="Export to Excel" OnClick="OnClickExportExcel"/>
                      <asp:Button CssClass="btn btn-danger" ID="ButtonPDF" runat="server" Text="Export to PDF" OnClick="OnClickExportPDF"/>
                    </div>
                </form>

                <!-- PANEL START -->
					<div class="panel panel-primary" id="panel_csc">
                   	  		<div class="panel-heading">
                                <table>
                                    <tr>
                                        <td style="padding:8px">
                                            <b>Planning Period:</b>
                                        </td>
                                        <td style="padding:8px">
                                            <div class="btn-group" role="group">
                                                <asp:PlaceHolder ID="PlaceHolderPeriod" runat="server"></asp:PlaceHolder>
                                            </div>
                                        </td>
                                        <td style="padding:8px">
                                            <b>Individual Stretch Rating: </b>
                                            <asp:Label ID="LabelStretchRating" runat="server" Text=""></asp:Label>%
                                        </td>
                            	        <td style="padding:8px">
                                            <b>Review: </b>
                                            <asp:Label ID="LabelReview" runat="server" Text=""></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <div class="table-responsive">
                                <table class="table table-bordered">
                                    <asp:PlaceHolder ID="PlaceHolderTable" runat="server"></asp:PlaceHolder>  
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
