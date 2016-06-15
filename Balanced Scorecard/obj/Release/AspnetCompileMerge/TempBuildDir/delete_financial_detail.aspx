<%@ Page Title="" Language="C#" MasterPageFile="~/BSC.Master" AutoEventWireup="true" CodeBehind="delete_financial_detail.aspx.cs" Inherits="Balanced_Scorecard.delete_financial_detail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
    Delete Multiple Financial Details | Balanced Scorecard
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper">
            <div class="container-fluid">
                    <div class="row">
                        <div class="col-lg-12">
                            <ol class="breadcrumb" id="breadcrumb-csc">
                              <li>Scorecard</li>
                              <li><a id="financial_measure_link" runat="server">Financial Measure</a></li>
                              <li><b>Delete Multiple <asp:Label ID="GroupName" runat="server" Text="Group Name"></asp:Label>'s Measures</b></li>
    						</ol>
                        	<h1><i class="fa fa fa-table"></i>&nbsp;Delete Multiple <asp:Label ID="GroupNameTitle" runat="server" Text="Group Name Title"></asp:Label>'s Measures</h1>
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
                        	<div class="panel-heading"><b>List of <asp:Label ID="GroupNameList" runat="server" Text="Group Name List">'s Details</asp:Label></b>
                            </div>
                            	<div class="table-responsive" style="overflow-x:visible">
                                    <asp:GridView OnRowDataBound="grv_OnRowDataBound" CssClass="table table-bordered table-gridview" ID="GridView1" runat="server" AutoGenerateColumns="false">
                                        <Columns>
                                            <asp:TemplateField HeaderText="No." HeaderStyle-CssClass="centering-th2" ItemStyle-HorizontalAlign="Center">
                                                <ItemTemplate>
                                                    <%# Container.DataItemIndex + 1 %>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialMeasure" HeaderText="Financial Measure" />
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialLinked" HeaderText="Financial Linked" />
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialType" HeaderText="Financial Type"/>
                                            <asp:TemplateField HeaderText="Target" HeaderStyle-CssClass="centering-th2" ItemStyle-HorizontalAlign="Center">
                                                <ItemTemplate>
                                                    <asp:Label ID="LabelTarget" runat="server" Text='<%# Eval("FinancialTarget") + " " %>'></asp:Label>
                                                    <asp:Label ID="LabelMeasureByTarget" runat="server" Text='<%# Eval("FinancialMeasureBy") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Result" HeaderStyle-CssClass="centering-th2" ItemStyle-HorizontalAlign="Center">
                                                <ItemTemplate>
                                                    <asp:Label ID="LabelResult" runat="server" Text='<%# Eval("FinancialResult") + " " %>'></asp:Label>
                                                    <asp:Label ID="LabelMeasureByResult" runat="server" Text='<%# Eval("FinancialMeasureBy") %>'></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialRating" DataFormatString="{0}%" HeaderText="Rating"/>
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialWeight" DataFormatString="{0}%" HeaderText="Weight (%)"/>
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialScore" DataFormatString="{0}%" HeaderText="Score"/>
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialFormula" HeaderText="Formula"/>
                                            <asp:BoundField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="centering-th2" DataField="FinancialRemarks" NullDisplayText="No Remarks" HeaderText="Remarks"/>
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
