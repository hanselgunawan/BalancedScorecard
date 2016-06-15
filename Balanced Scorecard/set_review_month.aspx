<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="set_review_month.aspx.cs" Inherits="Balanced_Scorecard.set_review_month" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Set Review Month | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><b>Set Review Month</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-calendar"></i>&nbsp;Set Review Month</h1>
                        </div>
                    </div>
                <!-- /.row -->
					<div class="panel panel-primary" id="panel_csc">
                        	<div class="panel-heading"><p class="csc_p">Scorecard Review Table</p></div>
                            	<div class="table-responsive" id="PeriodTable" runat="server">
                                    <table class="table table-bordered">
                                        <asp:PlaceHolder ID="PlaceHolderPeriod" runat="server"></asp:PlaceHolder>
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
