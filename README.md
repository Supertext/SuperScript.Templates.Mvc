_**IMPORTANT NOTE:**_ This project is currently in beta and the documentation is currently incomplete. Please bear with us while the documentation is being written.

####SuperScript offers a means of declaring assets in one part of a .NET web solution and have them emitted somewhere else.


When developing web solutions, assets such as JavaScript declarations or HTML templates are frequently written in a location that differs from their desired output location.

For example, all JavaScript declarations should ideally be emitted together just before the HTML document is closed. And if caching is preferred then these declarations should be in an external file with caching headers set.

This is the functionality offered by SuperScript.



##The Template Container Block

This project contains one publicly-accessible class, `SuperScript.Templates.Mvc.Containers.HtmlExtensions`,
which contains one overloaded method, `TemplateContainer()`. This method can be used on .NET Razor views for encapsulating an
HTML template and converting this to an instance of `SuperScript.Templates.Declarables.TemplateDeclaration`.

For example

```HTML
@using SuperScript.Templates.Mvc.Containers
...
@using (Html.TemplateContainer("demo-template", "templates"))
{
 <li>{{:Name}}</li>
}
```

In the above example the name of the client-side HTML template has been expressed in the `TemplateContainer` constructor.
An alternative allows multiple template declarations.

```HTML
@using SuperScript.Templates.Mvc.Containers
...
@using (Html.TemplateContainer(null, "templates"))
{
  <script type="text/html" id="template-1">
    <li>{{:Name}}</li>
  </script>

  <script type="text/html" id="template-2">
    <h1>{{:Name}}</h1>
  </script>
}
```


The overloads of the `TemplateContainer` constructor permit specifying
* the emitter key
* the name/id of the client-side template
* an index in the application-wide collection of `SuperScript.Declarables.DeclarationBase` objects at which the template(s)
in this block should be inserted



##Dependencies
There are a variety of SuperScript projects, some being dependent upon others.

* [`SuperScript.Common`](https://github.com/Supertext/SuperScript.Common)

  This library contains the core classes which facilitate all other SuperScript modules but which won't produce any meaningful output on its own.

* [`SuperScript.Container.Mvc`](https://github.com/Supertext/SuperScript.Container.Mvc)

  This library allows developers to easily declare these assets in MVC Razor views.

* [`SuperScript.Templates`](https://github.com/Supertext/SuperScript.Templates)

  This library contains functionality for making HTML template-specific declarations.
  

`SuperScript.Templates.Mvc` has been made available under the [MIT License](https://github.com/Supertext/SuperScript.Templates.Mvc/blob/master/LICENSE).
