<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Account.aspx.vb" Inherits="Account" %>
<%@ Register TagPrefix="uc1" TagName="Header" Src="~/Header.ascx" %>
<%@ Register TagPrefix="uc1" TagName="SysMenu" Src="~/SysMenu.ascx" %>
<%@ Register TagPrefix="uc1" TagName="Footer" Src="~/Footer.ascx" %>

<!DOCTYPE html>
<html>
 <head>
  <title>Account - My World</title>
  <link href="/styles/Site.css" type="text/css" rel="stylesheet" />
  <link href="/Styles/TopMenu.css" type="text/css" rel="stylesheet" />
  <link href="/styles/TreeView.css" type="text/css" rel="stylesheet" />
  <script type="text/javascript" src="/scripts/Cookie.js"></script>
  <script type="text/javascript" src="/scripts/TopMenu.js"></script>
  <script type="text/javascript" src="/scripts/TreeView.js"></script>
  <script type="text/javascript">

   function DoDel() {               // Forces a form post with events. Data must be in the form.
    document.getElementById('DelAccount').checked = true;
    setTimeout('__doPostBack(\'DelAccount\',\'\')', 0);
   }

   function ShowDelWin() {
    //alert('Show DelWin');
    document.getElementById("DivWinTrans").style.display = "block";
    document.getElementById("DivWinBox").style.display = "block";
   }

   function HideDelWin() {
    document.getElementById("DivWinBox").style.display = "none";
    document.getElementById("DivWinTrans").style.display = "none";
   }

  </script>
 </head>
 <body id="BodyTag" runat="server">
  <!-- Built from WebSys Add/Edit template v. 1.0 -->
  <div id="HeaderPos">
   <uc1:Header id="Header1" runat="server"></uc1:Header>
   <uc1:SysMenu id="SysMenu1" runat="server"></uc1:SysMenu>
  </div>
  <div id="LSideBar">
   <table class="SidebarCtl">
    <tr>
     <td class="ProgTitle">Account Settings</td>
    </tr>
    <tr>
     <td class="SidebarSpacer">&nbsp;</td>
    </tr>
    <!-- Sidebar Menu Control -->
    <tr>
     <td id="SidebarMenu" runat="server">
      <!-- Sidebar menu content is set in code -->
     </td>
    </tr>
   </table>
  </div>
  <div id="BodyArea">
   <table class="BodyTable">
    <tr>
     <td class="BodyBg">
      <!-- Body Content here -->
      <form id="aspnetForm" method="post" runat="server">
      <table style=" width:100%;">
       <tr> 
        <td class="PageTitle"> 
         Email Information
        </td>
       </tr>
       <tr>
        <td>
         <table style="width:100%;">
          <tr>
           <td class="FldTitleCol"></td>
           <td class="FldEntryCol">
            <b>Current Email:</b> <span id="CurrEmail" runat="server"></span><br />
            <b>Note:</b> Changing your email address will send a notice of the change to your old email address.
           </td>
          </tr>
          <tr>
           <td class="FldTitleCol">New Email Address</td>
           <td class="FldEntryCol">
            <asp:TextBox id="Email" runat="server" Columns="40" maxLength="40" CssClass="Form"/>
           </td>
          </tr>
          <tr> 
           <td colspan="2" style="text-align: center;" class="SubTitle">
            <asp:Button ID="Button1" Text="Change Email" runat="server"/>
           </td>
          </tr>
         </table>
        </td>
       </tr>
       <tr> 
        <td class="PageTitle"> 
         Account Password
        </td>
       </tr>
       <tr>
        <td>
         <table style="width:100%;">
          <tr>
           <td class="FldTitleCol">Current Password</td>
           <td class="FldEntryCol">
            <asp:TextBox id="Current" runat="server" TextMode="Password" Columns="15" maxLength="20" CssClass="Form"/>
           </td>
          </tr>
          <tr>
           <td class="FldTitleCol">New Password</td>
           <td class="FldEntryCol">
            <asp:TextBox id="NewPass" runat="server" TextMode="Password" Columns="15" maxLength="20" CssClass="Form"/>
           </td>
          </tr>
          <tr>
           <td class="FldTitleCol">Confirm Password</td>
           <td class="FldEntryCol">
            <asp:TextBox id="ConPass" runat="server" TextMode="Password" Columns="15" maxLength="20" CssClass="Form"/>
           </td>
          </tr>
          <tr> 
           <td colspan="2" style="text-align: center;" class="SubTitle">
            <asp:Button ID="Button2" Text="Reset Password" runat="server"/>
           </td>
          </tr>
         </table>
        </td>
       </tr>
       <tr> 
        <td class="PageTitle"> 
         Preference Settings
        </td>
       </tr>
       <tr>
        <td>
         <table style="width:100%;">
          <tr>
           <td class="FldTitleCol">
            Subscriptions
           </td>
           <td class="FldEntryCol">
            <asp:CheckBox ID="ImsInEmail" Checked="true" runat="server" Text="Receive Offline IMs in Email." />
            <asp:CheckBox ID="ListInDirect" runat="server" Text="List in Directory." /><br />
            <b>NOTE:</b> These may be set in viewer Preferences, Chat tab.
           </td>
          </tr>
          <tr> 
           <td colspan="2" class="SubTitle" style="text-align:center;">
            <asp:Button ID="Button3" Text="Set Preferences" runat="server"/>
           </td>
          </tr>
         </table>
        </td>
       </tr>
       <tr> 
        <td class="PageTitle"> 
         Partner Selection
        </td>
       </tr>
       <tr>
        <td>
         <table style="width:100%;">
          <tr id="PartnerSel" runat="server">
           <td class="FldTitleCol">Select a Partner</td>
           <td class="FldEntryCol">
            <b>Search:</b> <asp:TextBox id="Search" runat="server" Columns="40" maxLength="80" CssClass="Form" AutoPostBack="true"/><br />
            <b>Note:</b> Selecting a partner will send them an email notice to confirm or reject the offer.<br />
           </td>
          </tr>
          <tr id="PartFound" runat="server">
           <td class="FldTitleCol">Partner Search Results</td>
           <td class="FldEntryCol">
            <asp:DropDownList ID="Partners" runat="server">
            </asp:DropDownList>
           </td>
          </tr>
          <tr id="ShowPartner" runat="server">
           <td class="FldTitleCol">Current Partner:</td>
           <td id="CurrPartner" runat="server" class="FldEntryCol">
           </td>
          </tr>
          <tr> 
           <td colspan="2" style="text-align: center;" class="SubTitle">
            <asp:Button ID="Button4" Text="Send Partner Offer" runat="server"/>
            <asp:Button ID="Button5" Text="Remove Partner" runat="server"/>
           </td>
          </tr>
         </table>
        </td>
       </tr>
      </table>
       <asp:CheckBox ID="DelAccount" runat="server" AutoPostBack="true" CssClass="NoShow" />
      </form>
     </td>
    </tr>
   </table>
  </div>
  <div id="FooterPos">
   <uc1:Footer id="Footer" runat="server"></uc1:Footer>
  </div>
  <div id="DivWinTrans" runat="server" class="DivWinTrans"></div>
  <div id="DivWinBox" runat="server" class="DivWinBox">
   <div class="DivWin">
    <table style="width: 100%; height: 100%;">
     <tr>
      <td>
       <table class="WarnTable">
        <tr>
         <td style="height:25px;"> </td>
        </tr>
        <tr>
         <td class="WarnTitle">
          WARNING!
         </td>
        </tr>
        <tr>
         <td id="DelTitle" runat="server" class="WarnText">
         </td>
        </tr>
        <tr>
         <td class="WarnMsg">
          You are about to permanently remove your account!<br />
          This will also remove all your inventory and any funds in the account.
         </td>
        </tr>
        <tr>
         <td style="text-align: center;">
          <input type="button" value="Delete" onclick="DoDel();"/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
          <input type="button" value="Cancel" onclick="HideDelWin();"/>
         </td>
        </tr>
        <tr>
         <td style="height:25px;"> </td>
        </tr>
       </table>
      </td>
     </tr>
    </table>
   </div>
  </div>
 </body>
</html>
