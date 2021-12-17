namespace Fabulous.XamarinForms

open Fabulous
open Xamarin.Forms

type [<Struct>] AppThemeValues<'T> =
    { Light: 'T
      Dark: 'T voption }

module Attributes =
    /// Define an attribute storing a Widget for a bindable property
    let defineBindableWidget (bindableProperty: BindableProperty) =
        Attributes.defineWidget
            bindableProperty.PropertyName
            (fun target value ->
                let bindableObject = target :?> BindableObject
                if value = null then
                    bindableObject.ClearValue(bindableProperty)
                else
                    bindableObject.SetValue(bindableProperty, value)
            )

    let defineBindableWithComparer<'inputType, 'modelType> (bindableProperty: BindableProperty) (convert: 'inputType -> 'modelType) (compare: ('modelType * 'modelType) -> ScalarAttributeComparison) =
        Attributes.defineScalarWithConverter<'inputType, 'modelType>
            bindableProperty.PropertyName
            convert
            compare
            (fun (newValueOpt, node) ->
                let target = node.Target :?> BindableObject
                match newValueOpt with
                | ValueNone -> target.ClearValue(bindableProperty)
                | ValueSome v -> target.SetValue(bindableProperty, v)
            )

    let inline defineBindable<'T when 'T: equality> bindableProperty =
        defineBindableWithComparer<'T, 'T> bindableProperty id ScalarAttributeComparers.equalityCompare

    let inline defineAppThemeBindable<'T when 'T: equality> (bindableProperty: BindableProperty) =
        Attributes.defineScalarWithConverter<AppThemeValues<'T>, AppThemeValues<'T>>
            bindableProperty.PropertyName
            id
            ScalarAttributeComparers.equalityCompare
            (fun (newValueOpt, node) ->
                let target = node.Target :?> BindableObject
                match newValueOpt with
                | ValueNone -> target.ClearValue(bindableProperty)
                | ValueSome { Light = light; Dark = ValueNone } -> target.SetValue(bindableProperty, light)
                | ValueSome { Light = light; Dark = ValueSome dark } -> target.SetOnAppTheme(bindableProperty, light, dark)
            )
