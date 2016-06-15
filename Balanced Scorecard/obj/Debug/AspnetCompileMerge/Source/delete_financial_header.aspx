<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="delete_financial_header.aspx.cs" Inherits="Balanced_Scorecard.delete_financial_header" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Delete Financial Measure Header | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><a id="financial_measure_link" runat="server">Financial Measure</a></li>
                              <li><b>Delete Multiple Financial Groups</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Delete Multiple Financial Groups</h1>
                        </div>
                    </div>
                <!-- /.row -->
                 <form id="form1" runat="server">
                    <div style="text-align:left">
                        <a id="back_link" runat="server" class="btn btn-primary" style="margin-bottom:-90px; transition:all ease 0.2s"><i class="fa fa-fw fa-reply"></i>&nbsp;Back to Financial Measure</a>
                    </div>
                    <div style="text-align:right">
                        <span style="color:white" class="btn btn-add-group btn-delete-financial-header-container add-button" id="SpanAddGroup" runat="server">
                            <i class="fa fa-fw fa-trash-o"></i>
                            <asp:Button BackColor="Transparent" BorderColor="Transparent" ID="ButtonDelete" CssClass="relative-to-btn-delete" OnClientClick="return confirm('Are you sure you want to delete?')" OnClick="OnClickDelete" runat="server" Text="Delete" />
                        </span>
                    </div>
                     <p style="margin-top:15px; margin-bottom:-8px"><b>Planning Period: </b><asp:Label ID="LabelPeriod" runat="server" Text="Period"></asp:Label></p>
                    <div class="panel panel-primary" id="panel_csc">
                        	<div class="panel-heading"><b>List of Financial Groups</b>
                            </div>
                            	<div class="table-responsive" style="overflow-x:visible">
                                    <asp:GridView CssClass="table table-bordered table-gridview" ID="GridView1" runat="server" AutoGenerateColumns="false">
                                        <Columns>
                                            <asp:TemplateField HeaderText="No." HeaderStyle-CssClass="centering-th2" ItemStyle-HorizontalAlign="Center">
                                                <ItemTemplate>
                                                    <%# Container.DataItemIndex + 1 %>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialHeader_Group" HeaderText="Group Name" />
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialHeader_StretchRating" HeaderText="Stretch Rating"/>
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialHeader_Review" HeaderText="Review"/>
                                            <asp:TemplateField HeaderText="Checkbox" HeaderStyle-CssClass="centering-th2" ItemStyle-HorizontalAlign="Center">
                                                <ItemTemplate>
                                                    <asp:CheckBox ID="chkCtrl" runat="server" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                               </div>
                    </div>
                 </form>
            </div>
            <!-- /.container-fluid -->

        </div>
        <!-- /#page-wrapper -->
</asp:Content>
