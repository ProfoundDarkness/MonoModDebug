/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Collections.Generic;
#if CODEPLEX_40
using System.Dynamic.Utils;
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Utils;
using Microsoft.Linq.Expressions;
#endif
using System.Reflection;

#if SILVERLIGHT
using System.Core;
#else
using System.Runtime.Remoting;
#endif

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    /// <summary>
    /// Represents the dynamic binding and a binding logic of an object participating in the dynamic binding.
    /// </summary>
    public class DynamicMetaObject {
        private readonly Expression _expression;
        private readonly BindingRestrictions _restrictions;
        private readonly object _value;
        private readonly bool _hasValue;

        /// <summary>
        /// Represents an empty array of type <see cref="DynamicMetaObject"/>. This field is read only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly DynamicMetaObject[] EmptyMetaObjects = new DynamicMetaObject[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicMetaObject"/> class.
        /// </summary>
        /// <param name="expression">The expression representing this <see cref="DynamicMetaObject"/> during the dynamic binding process.</param>
        /// <param name="restrictions">The set of binding restrictions under which the binding is valid.</param>
        public DynamicMetaObject(Expression expression, BindingRestrictions restrictions) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(restrictions, "restrictions");

            _expression = expression;
            _restrictions = restrictions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicMetaObject"/> class.
        /// </summary>
        /// <param name="expression">The expression representing this <see cref="DynamicMetaObject"/> during the dynamic binding process.</param>
        /// <param name="restrictions">The set of binding restrictions under which the binding is valid.</param>
        /// <param name="value">The runtime value represented by the <see cref="DynamicMetaObject"/>.</param>
        public DynamicMetaObject(Expression expression, BindingRestrictions restrictions, object value)
            : this(expression, restrictions) {
            _value = value;
            _hasValue = true;
        }

        /// <summary>
        /// The expression representing the <see cref="DynamicMetaObject"/> during the dynamic binding process.
        /// </summary>
        public Expression Expression {
            get {
                return _expression;
            }
        }

        /// <summary>
        /// The set of binding restrictions under which the binding is valid.
        /// </summary>
        public BindingRestrictions Restrictions {
            get {
                return _restrictions;
            }
        }

        /// <summary>
        /// The runtime value represented by this <see cref="DynamicMetaObject"/>.
        /// </summary>
        public object Value {
            get {
                return _value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="DynamicMetaObject"/> has the runtime value.
        /// </summary>
        public bool HasValue {
            get {
                return _hasValue;
            }
        }


        /// <summary>
        /// Gets the <see cref="Type"/> of the runtime value or null if the <see cref="DynamicMetaObject"/> has no value associated with it.
        /// </summary>
        public Type RuntimeType {
            get {
                if (_hasValue) {
                    Type ct = Expression.Type;
                    // valuetype at compile tyme, type cannot change.
                    if (ct.IsValueType) {
                        return ct;
                    }
                    if (_value != null) {
                        return _value.GetType();
                    } else {
                        return null;
                    }
                } else {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the limit type of the <see cref="DynamicMetaObject"/>.
        /// </summary>
        /// <remarks>Represents the most specific type known about the object represented by the <see cref="DynamicMetaObject"/>. <see cref="RuntimeType"/> if runtime value is available, a type of the <see cref="Expression"/> otherwise.</remarks>
        public Type LimitType {
            get {
                return RuntimeType ?? Expression.Type;
            }
        }

        /// <summary>
        /// Performs the binding of the dynamic conversion operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="ConvertBinder"/> that represents the details of the dynamic operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindConvert(ConvertBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackConvert(this);
        }

        /// <summary>
        /// Performs the binding of the dynamic get member operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetMember(this);
        }

        /// <summary>
        /// Performs the binding of the dynamic set member operation. 
        /// </summary>
        /// <param name="binder">An instance of the <see cref="SetMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value for the set member operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetMember(this, value);
        }

        /// <summary>
        /// Performs the binding of the dynamic delete member operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="DeleteMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteMember(this);
        }

        /// <summary>
        /// Performs the binding of the dynamic get index operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="GetIndexBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="indexes">An array of <see cref="DynamicMetaObject"/> instances - indexes for the get index operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackGetIndex(this, indexes);
        }

        /// <summary>
        /// Performs the binding of the dynamic set index operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="SetIndexBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="indexes">An array of <see cref="DynamicMetaObject"/> instances - indexes for the set index operation.</param>
        /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value for the set index operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackSetIndex(this, indexes, value);
        }

        /// <summary>
        /// Performs the binding of the dynamic delete index operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="DeleteIndexBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="indexes">An array of <see cref="DynamicMetaObject"/> instances - indexes for the delete index operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackDeleteIndex(this, indexes);
        }

        /// <summary>
        /// Performs the binding of the dynamic invoke member operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="InvokeMemberBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="args">An array of <see cref="DynamicMetaObject"/> instances - arguments to the invoke member operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvokeMember(this, args);
        }

        /// <summary>
        /// Performs the binding of the dynamic invoke operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="InvokeBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="args">An array of <see cref="DynamicMetaObject"/> instances - arguments to the invoke operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackInvoke(this, args);
        }

        /// <summary>
        /// Performs the binding of the dynamic create instance operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="CreateInstanceBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="args">An array of <see cref="DynamicMetaObject"/> instances - arguments to the create instance operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackCreateInstance(this, args);
        }

        /// <summary>
        /// Performs the binding of the dynamic unary operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="UnaryOperationBinder"/> that represents the details of the dynamic operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackUnaryOperation(this);
        }

        /// <summary>
        /// Performs the binding of the dynamic binary operation.
        /// </summary>
        /// <param name="binder">An instance of the <see cref="BinaryOperationBinder"/> that represents the details of the dynamic operation.</param>
        /// <param name="arg">An instance of the <see cref="DynamicMetaObject"/> representing the right hand side of the binary operation.</param>
        /// <returns>The new <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
        public virtual DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
            ContractUtils.RequiresNotNull(binder, "binder");
            return binder.FallbackBinaryOperation(this, arg);
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>The list of dynamic member names.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetDynamicMemberNames() {
            return new string[0];
        }

        /// <summary>
        /// Returns the list of expressions represented by the <see cref="DynamicMetaObject"/> instances.
        /// </summary>
        /// <param name="objects">An array of <see cref="DynamicMetaObject"/> instances to extract expressions from.</param>
        /// <returns>The array of expressions.</returns>
        internal static Expression[] GetExpressions(DynamicMetaObject[] objects) {
            ContractUtils.RequiresNotNull(objects, "objects");

            Expression[] res = new Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                DynamicMetaObject mo = objects[i];
                ContractUtils.RequiresNotNull(mo, "objects");
                Expression expr = mo.Expression;
                ContractUtils.RequiresNotNull(expr, "objects");
                res[i] = expr;
            }

            return res;
        }

        /// <summary>
        /// Creates a meta-object for the specified object. 
        /// </summary>
        /// <param name="value">The object to get a meta-object for.</param>
        /// <param name="expression">The expression representing this <see cref="DynamicMetaObject"/> during the dynamic binding process.</param>
        /// <returns>
        /// If the given object implements <see cref="IDynamicMetaObjectProvider"/> and is not a remote object from outside the current AppDomain,
        /// returns the object's specific meta-object returned by <see cref="IDynamicMetaObjectProvider.GetMetaObject"/>. Otherwise a plain new meta-object 
        /// with no restrictions is created and returned.
        /// </returns>
        public static DynamicMetaObject Create(object value, Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");

            IDynamicMetaObjectProvider ido = value as IDynamicMetaObjectProvider;
#if !SILVERLIGHT
            if (ido != null && !RemotingServices.IsObjectOutOfAppDomain(value)) {
#else
            if (ido != null) {
#endif
                var idoMetaObject = ido.GetMetaObject(expression);

                if (idoMetaObject == null ||
                    !idoMetaObject.HasValue ||
                    idoMetaObject.Value == null ||
                    (object)idoMetaObject.Expression != (object)expression) {
                    throw Error.InvalidMetaObjectCreated(ido.GetType());
                }

                return idoMetaObject;
            } else {
                return new DynamicMetaObject(expression, BindingRestrictions.Empty, value);
            }
        }
    }
}
