using System;
using System.Linq;
using System.Web.Mvc;
using HtmlAgilityPack;
using SuperScript.Configuration;
using SuperScript.Container.Mvc;
using SuperScript.Templates.Declarables;
using SuperScript.Templates.Exceptions;

namespace SuperScript.Templates.Mvc.Containers
{
    public static class HtmlExtensions
    {
        private class InternalTemplateContainer : BaseContainer
        {
            /// <summary>
            /// Gets or sets the name of the template.
            /// </summary>
            private string Name { get; set; }


            // only calls the constructor on the base class
            public InternalTemplateContainer(HtmlHelper helper, string emitterKey, string name, int? insertAt)
                : base(helper, emitterKey, insertAt)
            {
                Name = name;
            }


            /// <summary>
            /// Append the internal content to the context's cached list of output delegates
            /// </summary>
            public override void Dispose()
            {
                // render the internal content of the injection block helper
                // make sure to pop from the stack rather than just render from the Writer
                // so it will remove it from regular rendering
                var content = WebPage.OutputStack;
                var renderedContent = content.Count == 0
                                          ? string.Empty
                                          : content.Pop().ToString();

                // load this into an HtmlAgility document.
                // - if a <script> tag is present then HtmlAgility will recognise this and we can request the InnerText
                var doc = new HtmlDocument();
                doc.LoadHtml(renderedContent);

                var textNodeAdded = false;

                foreach (var node in doc.DocumentNode.ChildNodes.Where(node => node.NodeType == HtmlNodeType.Element))
                {
                    if (node.Name.Equals("script", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (String.IsNullOrWhiteSpace(node.InnerHtml.Trim()))
                        {
                            continue;
                        }

                        var decl = new TemplateDeclaration();

                        // use the 'id' or 'name' attribute as this template's name
                        var attrTemplName = node.GetAttributeValue("id", null);
                        if (attrTemplName == null)
                        {
                            attrTemplName = node.GetAttributeValue("name", null);
                            if (attrTemplName == null)
                            {
                                throw new MissingTemplateInformationException("A TemplateDeclaration requires a name to be specified.");
                            }
                        }

                        decl.Name = attrTemplName;

                        // check if an EmitterKey (with greater specificity than being declared on the TemplateContainer) has been specified
                        decl.EmitterKey = node.GetAttributeValue("emitterKey", EmitterKey);

                        decl.Template = node.InnerHtml.Trim();

                        var attrInsertAt = node.GetAttributeValue("insertAt", -1);
                        if (attrInsertAt > -1)
                        {
                            Declarations.AddDeclaration<TemplateDeclaration>(decl, attrInsertAt);
                        }

                        Declarations.AddDeclaration<TemplateDeclaration>(decl, InsertAt);
                    }
                    else
                    {
                        if (textNodeAdded)
                        {
                            throw new DuplicateTemplateException("Multiple instances of TemplateDeclaration cannot be added inside a TemplateContainer unless they are contained within <script> tags, each with their own 'name' or 'id' property.");
                        }

                        if (String.IsNullOrWhiteSpace(Name))
                        {
                            throw new MissingTemplateInformationException("A TemplateDeclaration requires a name to be specified.");
                        }

                        if (String.IsNullOrWhiteSpace(EmitterKey))
                        {
                            throw new MissingTemplateInformationException("A TemplateDeclaration requires an emitter key to be specified.");
                        }

                        textNodeAdded = true;

                        Declarations.AddDeclaration<TemplateDeclaration>(new TemplateDeclaration
                                                                             {
                                                                                 Name = Name,
                                                                                 Template = node.InnerHtml.Trim(),
                                                                                 EmitterKey = EmitterKey
                                                                             }, InsertAt);
                    }
                }
            }
        }


        /// <summary>
        /// Start a block of movable HTML template content.
        /// </summary>
        /// <param name="helper">
        /// The helper from which we use the context.
        /// </param>
        /// <returns>
        /// This extension method will return an empty <see cref="string"/> at runtime.
        /// </returns>
        public static IDisposable TemplateContainer(this HtmlHelper helper)
        {
            return new InternalTemplateContainer(helper,
                                                 Settings.Instance.DefaultEmitter.Key,
                                                 null,  // template name
                                                 null); // insertAt
        }


        /// <summary>
        /// Start a block of movable HTML template content.
        /// </summary>
        /// <param name="helper">
        /// The helper from which we use the context.
        /// </param>
        /// <param name="name">The client-side name assigned to this template.</param>
        /// <returns>
        /// This extension method will return an empty <see cref="string"/> at runtime.
        /// </returns>
        public static IDisposable TemplateContainer(this HtmlHelper helper, string name)
        {
            return new InternalTemplateContainer(helper,
                                                 Settings.Instance.DefaultEmitter.Key,
                                                 name,
                                                 null); // insertAt
        }


        /// <summary>
        /// Start a block of movable HTML template content.
        /// </summary>
        /// <param name="helper">
        /// The helper from which we use the context.
        /// </param>
        /// <param name="name">The client-side name assigned to this template.</param>
        /// <param name="emitterKey">
        /// <para>Indicates which instance of <see cref="SuperScript.Emitters.IEmitter"/> the content should be added to.</para>
        /// <para>If not specified then the contents will be added to the default implementation of <see cref="SuperScript.Emitters.IEmitter"/>.</para>
        /// </param>
        /// <returns>
        /// This extension method will return an empty <see cref="string"/> at runtime.
        /// </returns>
        public static IDisposable TemplateContainer(this HtmlHelper helper, string name, string emitterKey)
        {
            return new InternalTemplateContainer(helper,
                                                 emitterKey,
                                                 name,  // template name
                                                 null); // insertAt
        }


        /// <summary>
        /// Start a block of movable HTML template content.
        /// </summary>
        /// <param name="helper">
        /// The helper from which we use the context.
        /// </param>
        /// <param name="insertAt">Indicates the index in the collection at which the contents are to be inserted.</param>
        /// <returns>
        /// This extension method will return an empty <see cref="string"/> at runtime.
        /// </returns>
        public static IDisposable TemplateContainer(this HtmlHelper helper, int insertAt)
        {
            return new InternalTemplateContainer(helper,
                                                 Settings.Instance.DefaultEmitter.Key,
                                                 null,      // template name
                                                 insertAt);
        }


        /// <summary>
        /// Start a block of movable HTML template content.
        /// </summary>
        /// <param name="helper">
        /// The helper from which we use the context.
        /// </param>
        /// <param name="name">The client-side name assigned to this template.</param>
        /// <param name="insertAt">Indicates the index in the collection at which the contents are to be inserted.</param>
        /// <returns>
        /// This extension method will return an empty <see cref="string"/> at runtime.
        /// </returns>
        public static IDisposable TemplateContainer(this HtmlHelper helper, string name, int insertAt)
        {
            return new InternalTemplateContainer(helper,
                                                 Settings.Instance.DefaultEmitter.Key,
                                                 name,
                                                 insertAt);
        }


        /// <summary>
        /// Start a block of movable HTML template content.
        /// </summary>
        /// <param name="helper">
        /// The helper from which we use the context.
        /// </param>
        /// <param name="name">The client-side name assigned to this template.</param>
        /// <param name="emitterKey">
        /// <para>Indicates which instance of <see cref="SuperScript.Emitters.IEmitter"/> the content should be added to.</para>
        /// <para>If not specified then the contents will be added to the default implementation of <see cref="SuperScript.Emitters.IEmitter"/>.</para>
        /// </param>
        /// <param name="insertAt">Indicates the index in the collection at which the contents are to be inserted.</param>
        /// <returns>
        /// This extension method will return an empty <see cref="string"/> at runtime.
        /// </returns>
        public static IDisposable TemplateContainer(this HtmlHelper helper, string name, string emitterKey, int insertAt)
        {
            return new InternalTemplateContainer(helper,
                                                 emitterKey,
                                                 name,      // template name
                                                 insertAt);
        }
    }
}