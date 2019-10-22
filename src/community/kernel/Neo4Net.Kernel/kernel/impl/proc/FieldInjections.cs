using System;
using System.Collections.Generic;
using System.Reflection;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.proc
{

	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using ComponentInjectionException = Neo4Net.Kernel.Api.Exceptions.ComponentInjectionException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Context = Neo4Net.Procedure.Context;

	/// <summary>
	/// Injects annotated fields with appropriate values.
	/// </summary>
	internal class FieldInjections
	{
		 private readonly ComponentRegistry _components;

		 internal FieldInjections( ComponentRegistry components )
		 {
			  this._components = components;
		 }

		 /// <summary>
		 /// On calling apply, injects the `value` for the field `field` on the provided `object`.
		 /// </summary>
		 internal class FieldSetter
		 {
			  internal readonly System.Reflection.FieldInfo Field;
			  internal readonly MethodHandle Setter;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final ComponentRegistry.Provider<?> provider;
			  internal readonly ComponentRegistry.Provider<object> Provider;

			  internal FieldSetter<T1>( System.Reflection.FieldInfo field, MethodHandle setter, ComponentRegistry.Provider<T1> provider )
			  {
					this.Field = field;
					this.Setter = setter;
					this.Provider = provider;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void apply(org.Neo4Net.kernel.api.proc.Context ctx, Object object) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
			  internal virtual void Apply( Neo4Net.Kernel.api.proc.Context ctx, object @object )
			  {
					try
					{
						 Setter.invoke( @object, Provider.apply( ctx ) );
					}
					catch ( Exception e )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, e, "Unable to inject component to field `%s`, please ensure it is public and non-final: %s", Field.Name, e.Message );
					}
			  }
		 }

		 /// <summary>
		 /// For each annotated field in the provided class, creates a `FieldSetter`. </summary>
		 /// <param name="cls"> The class where injection should happen. </param>
		 /// <returns> A list of `FieldSetters` </returns>
		 /// <exception cref="ProcedureException"> if the type of the injected field does not match what has been registered. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<FieldSetter> setters(Class cls) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 internal virtual IList<FieldSetter> Setters( Type cls )
		 {
			  IList<FieldSetter> setters = new LinkedList<FieldSetter>();
			  Type currentClass = cls;

			  do
			  {
					foreach ( System.Reflection.FieldInfo field in currentClass.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ) )
					{
						 //ignore synthetic fields
						 if ( field.Synthetic )
						 {
							  continue;
						 }
						 if ( Modifier.isStatic( field.Modifiers ) )
						 {
							  if ( field.isAnnotationPresent( typeof( Context ) ) )
							  {
									throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "The field `%s` in the class named `%s` is annotated as a @Context field,%n" + "but it is static. @Context fields must be public, non-final and non-static,%n" + "because they are reset each time a procedure is invoked.", field.Name, cls.Name );
							  }
							  continue;
						 }

						 AssertValidForInjection( cls, field );
						 setters.Add( CreateInjector( cls, field ) );
					}
			  } while ( ( currentClass = currentClass.BaseType ) != null );

			  return setters;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private FieldSetter createInjector(Class cls, Field field) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private FieldSetter CreateInjector( Type cls, System.Reflection.FieldInfo field )
		 {
			  try
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ComponentRegistry.Provider<?> provider = components.providerFor(field.getType());
					ComponentRegistry.Provider<object> provider = _components.providerFor( field.Type );
					if ( provider == null )
					{
						 throw new ComponentInjectionException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Unable to set up injection for procedure `%s`, the field `%s` " + "has type `%s` which is not a known injectable component.", cls.Name, field.Name, field.Type );
					}

					MethodHandle setter = MethodHandles.lookup().unreflectSetter(field);
					return new FieldSetter( field, setter, provider );
			  }
			  catch ( IllegalAccessException e )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Unable to set up injection for `%s`, failed to access field `%s`: %s", e, cls.Name, field.Name, e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertValidForInjection(Class cls, Field field) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private void AssertValidForInjection( Type cls, System.Reflection.FieldInfo field )
		 {
			  if ( !field.isAnnotationPresent( typeof( Context ) ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Field `%s` on `%s` is not annotated as a @" + typeof( Context ).Name + " and is not static. If you want to store state along with your procedure," + " please use a static field.", field.Name, cls.Name );
			  }

			  if ( !Modifier.isPublic( field.Modifiers ) || Modifier.isFinal( field.Modifiers ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Field `%s` on `%s` must be non-final and public.", field.Name, cls.Name );

			  }
		 }
	}

}