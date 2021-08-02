﻿#pragma checksum "..\..\..\ThreeStepSwitch.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "4FE14FDB957354B39D6910492461D0494AA8F2DBFA7DB29E4041035BB814E48D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Frank_UI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Frank_UI {
    
    
    /// <summary>
    /// ThreeStepSwitch
    /// </summary>
    public partial class ThreeStepSwitch : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\ThreeStepSwitch.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grdMain;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\ThreeStepSwitch.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border bdExclude;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\ThreeStepSwitch.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border bdNeutral;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\ThreeStepSwitch.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border bdInclude;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\ThreeStepSwitch.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas btnSwitch;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\ThreeStepSwitch.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grdPlus;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\ThreeStepSwitch.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grdMinus;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Frank UI;component/threestepswitch.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\ThreeStepSwitch.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.grdMain = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.bdExclude = ((System.Windows.Controls.Border)(target));
            
            #line 20 "..\..\..\ThreeStepSwitch.xaml"
            this.bdExclude.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.bdExclude_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.bdNeutral = ((System.Windows.Controls.Border)(target));
            
            #line 21 "..\..\..\ThreeStepSwitch.xaml"
            this.bdNeutral.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.bdNeutral_MouseDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.bdInclude = ((System.Windows.Controls.Border)(target));
            
            #line 22 "..\..\..\ThreeStepSwitch.xaml"
            this.bdInclude.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.bdInclude_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnSwitch = ((System.Windows.Controls.Canvas)(target));
            
            #line 23 "..\..\..\ThreeStepSwitch.xaml"
            this.btnSwitch.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.btnSwitch_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 23 "..\..\..\ThreeStepSwitch.xaml"
            this.btnSwitch.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.btnSwitch_MouseLeftButtonUp);
            
            #line default
            #line hidden
            
            #line 23 "..\..\..\ThreeStepSwitch.xaml"
            this.btnSwitch.MouseMove += new System.Windows.Input.MouseEventHandler(this.btnSwitch_MouseMove);
            
            #line default
            #line hidden
            
            #line 23 "..\..\..\ThreeStepSwitch.xaml"
            this.btnSwitch.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.btnSwitch_PreviewMouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 23 "..\..\..\ThreeStepSwitch.xaml"
            this.btnSwitch.PreviewMouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.btnSwitch_PreviewMouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 6:
            this.grdPlus = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.grdMinus = ((System.Windows.Controls.Grid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

