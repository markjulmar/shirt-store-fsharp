namespace XamarinStore

open System
open System.Linq
open System.Collections.Generic
open MonoTouch.UIKit
open System.Drawing

type PickerModel() =
    inherit UIPickerViewModel()

    member val Parent: StringUIPicker = null with get, set
    member val Items: string array = [||] with get, set

    override this.GetComponentCount picker =
        1

    override this.GetRowsInComponent (picker, ``component``) =
        this.Items.Length

    override this.GetTitle (picker, row, ``component``) =
        this.Items.[row]

    override this.Selected (picker, row, ``component``) =
        if this.Parent <> null then
            this.Parent.SelectedIndex <- row

and [<AllowNullLiteralAttribute>] StringUIPicker() =
    inherit UIPickerView()

    let mutable currentIndex = 0
    let mutable items :string array = [||]
    let mutable sheet : UIView = null

    member this.Items
        with get () = items :> string seq
        and set (value:string seq) = items <- value.ToArray()
                                     this.Model <- new PickerModel(Items = items, Parent = this)

    member val SelectedItemChanged = fun x->() with get,set

    member this.SelectedIndex
        with get () = currentIndex
        and set value =
            if currentIndex <> value then
                currentIndex <- value
                this.Select (currentIndex, 0, true)
                this.SelectedItemChanged this

    member this.SelectedItem
        with get () = if items.Length <= currentIndex then "" else items.[currentIndex]
        and set value = if items.Contains(value) then
                            currentIndex <- Array.FindIndex(items, fun x-> x = value)

    member this.ShowPicker() =
        sheet <- new UIView()
        sheet.BackgroundColor <- UIColor.Clear

        let parentView = UIApplication.SharedApplication.KeyWindow.RootViewController.View

        // Creates a transparent grey background who catches the touch actions (and add more style). 
        let dimBackgroundView = new UIView (parentView.Bounds)
        dimBackgroundView.BackgroundColor <- UIColor.Gray.ColorWithAlpha (0.5f)

        let titleBarHeight = 44.0f
        let actionSheetSize = new SizeF (parentView.Frame.Width, this.Frame.Height + titleBarHeight)
        let actionSheetFrameHidden = new RectangleF (0.0f, parentView.Frame.Height, actionSheetSize.Width, actionSheetSize.Height);
        let actionSheetFrameDisplayed = new RectangleF (0.0f, parentView.Frame.Height - actionSheetSize.Height, actionSheetSize.Width, actionSheetSize.Height);

        // Hide the action sheet before we animate it so it comes from the bottom.
        sheet.Frame <- actionSheetFrameHidden;
        this.Frame <- new RectangleF (0.0f, 1.0f, actionSheetSize.Width, actionSheetSize.Height - titleBarHeight);

        let doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done)

        doneButton.Clicked.Add(fun evArgs -> UIView.Animate(0.25, (fun () -> sheet.Frame <- actionSheetFrameHidden), (fun () -> (dimBackgroundView.RemoveFromSuperview() ; sheet.RemoveFromSuperview()))))

        let toolbarPicker = new UIToolbar (new RectangleF (0.0f, 0.0f, sheet.Frame.Width, titleBarHeight))
        toolbarPicker.ClipsToBounds <- true
        toolbarPicker.Items <- [|new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace); doneButton|]
        toolbarPicker.BarTintColor <- this.BackgroundColor

        // Creates a blur background using the toolbar trick.
        let toolbarBg = new UIToolbar (new RectangleF (0.f, 0.f, sheet.Frame.Width, sheet.Frame.Height))
        toolbarBg.ClipsToBounds <- true

        sheet.AddSubviews(toolbarBg, this, toolbarPicker)
        parentView.AddSubviews(dimBackgroundView, sheet)
        parentView.BringSubviewToFront (sheet)
        UIView.Animate(0.25, fun () -> sheet.Frame <- actionSheetFrameDisplayed)