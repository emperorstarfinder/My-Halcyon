
Partial Class PopupError
 Inherits System.Web.UI.Page

 '*************************************************************************************************
 '* Open Source Project Notice:
 '* The "MyWorld" website is a community supported open source project intended for use with the 
 '* Halcyon Simulator project posted at https://github.com/HalcyonGrid and compatible derivatives of 
 '* that work. 
 '* Contributions to the MyWorld website project are to be original works contributed by the authors
 '* or other open source projects. Only the works that are directly contributed to this project are
 '* considered to be part of the project, included in it as community open source content. This does 
 '* not include separate projects or sources used and owned by the respective contributors that may 
 '* contain similar code used in their other works. Each contribution to the MyWorld project is to 
 '* include in a header like this what its sources and contributor are and any applicable exclusions 
 '* from this project. 
 '* The MyWorld website is released as public domain content is intended for Halcyon Simulator 
 '* virtual world support. It is provided as is and for use in customizing a website access and 
 '* support for the intended application and may not be suitable for any other use. Any derivatives 
 '* of this project may not reverse claim exclusive rights or profiting from this work. 
 '*************************************************************************************************
 '* This page is the general template for any called popup pages needed from any other website pages.
 '* 
 '* 

 '* Built from MyWorld Popup Page template v. 1.0

 ' Define common properties and objects here for the page
 Private SQLCmd As String

 Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

  Trace.IsEnabled = False
  If Trace.IsEnabled Then Trace.Warn("PopupError", "Start Page Load")

  If Not IsPostBack Then                                   ' First time page is called setup

   ' Set Display texts
   PopTitle.InnerText = "Page Error"

   ' Setup general page controls

   ' define local objects here
   Message.InnerText = Session("Message")
  End If

 End Sub

 Private Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Unload
  ' Close open page objects
 End Sub

End Class
