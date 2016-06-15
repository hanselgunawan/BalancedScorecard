<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="set_period.aspx.cs" Inherits="Balanced_Scorecard.set_period" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Set BSC Period | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">

            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li><b>Set BSC Period</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-calendar"></i>&nbsp;Set BSC Period</h1>
                        </div>
                    </div>
                <!-- /.row -->
            <a id="AddNewBSCPeriod" runat="server" class="btn btn-default" style="margin-top:20px"><i class="fa fa fa-plus"></i>&nbsp;&nbsp;Add New BSC	Period</a>
                
                <p id="WarningActiveMessage" runat="server"><b>There is no Active period. Please activate a period.</b></p>
					<div class="panel panel-primary" id="panel_csc">
                        	<div class="panel-heading"><p class="csc_p">Planning Period Table</p></div>
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
