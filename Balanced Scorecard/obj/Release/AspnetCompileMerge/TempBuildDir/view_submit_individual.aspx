<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="view_submit_individual.aspx.cs" Inherits="Balanced_Scorecard.view_submit_individual" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    View Submit Individual | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">

            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><a id="hrefDashboard" runat="server">Scorecard Dashboard</a></li>
                              <li><a id="hrefSubmitUsers" runat="server">View Submit Users</a></li>
                              <li><b>View Individual Scorecard</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;<asp:Label ID="LabelUserTitle" runat="server" Text="Label"></asp:Label> (<asp:Label ID="LabelNIKTitle" runat="server" Text="Label"></asp:Label>)'s Individual Scorecard</h1>
                        </div>
                    </div>
                <!-- /.row -->
                
                <a id="btnBackToSubmitUsers" runat="server" class="btn btn-primary" style="margin-top:20px"><i class="fa fa fa-reply"></i>&nbsp;&nbsp;Back to View Submit Users</a>
                <form id="FormExport" runat="server">
                    <div style="text-align:right; margin-top:-35px">
                        
                      <asp:Button CssClass="btn btn-danger" ID="ButtonPDF" runat="server" Text="Export to PDF" OnClick="OnClickExportPDF"/>
                      <asp:Button CssClass="btn btn-success" ID="ButtonExcel" runat="server" Text="Export to Excel" OnClick="OnClickExportExcel"/>
                    </div>
                </form>
                <p style="margin-top:30px; margin-bottom:-10px"><b>Period:</b> <asp:Label ID="LabelPeriod" runat="server" Text="Label"></asp:Label></p>
                <!-- PANEL START -->
					<div class="panel panel-primary" id="panel_csc">
                   	  		<div class="panel-heading">
                                <table>
                                    <tr>
                                        <td style="padding-right:20px" align="center"><b><asp:Label ID="LabelUserPanel" runat="server" Text="Label"></asp:Label> (<asp:Label ID="LabelNIKPanel" runat="server" Text="Label"></asp:Label>)'s Individual Scorecard</b></td>
                            	        <td style="border-right:1px solid white; border-left:1px solid white; padding-right:20px"><span style="margin-left:20px"><b>Stretch Rating: </b>
                                            <asp:Label ID="LabelStretchRating" runat="server" Text="Label"></asp:Label></span></td>
                                        <td><span style="margin-left:20px"><b>Review: </b>
                                            <asp:Label ID="LabelReview" runat="server" Text="Label"></asp:Label></span></td>
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
