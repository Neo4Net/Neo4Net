using System;
using System.Collections.Generic;
using System.Reflection;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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

	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using FieldSignature = Neo4Net.@internal.Kernel.Api.procs.FieldSignature;
	using ProcedureSignature = Neo4Net.@internal.Kernel.Api.procs.ProcedureSignature;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;


	/// <summary>
	/// Takes user-defined record classes, and does two things: Describe the class as a <seealso cref="ProcedureSignature"/>,
	/// and provide a mechanism to convert an instance of the class to Neo4j-typed Object[].
	/// </summary>
	public class OutputMappers
	{
		 public OutputMappers( TypeMappers typeMappers )
		 {
			  this._typeMappers = typeMappers;
		 }

		 /// <summary>
		 /// A compiled mapper, takes an instance of a java class, and converts it to an Object[] matching
		 /// the specified <seealso cref="signature()"/>.
		 /// </summary>
		 public class OutputMapper
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<FieldSignature> SignatureConflict;
			  internal readonly FieldMapper[] FieldMappers;

			  public OutputMapper( FieldSignature[] signature, FieldMapper[] fieldMappers )
			  {
					this.SignatureConflict = new IList<FieldSignature> { signature };
					this.FieldMappers = fieldMappers;
			  }

			  public virtual IList<FieldSignature> Signature()
			  {
					return SignatureConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object[] apply(Object record) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  public virtual object[] Apply( object record )
			  {
					object[] output = new object[FieldMappers.Length];
					for ( int i = 0; i < FieldMappers.Length; i++ )
					{
						 output[i] = FieldMappers[i].apply( record );
					}
					return output;
			  }
		 }

		 private static readonly OutputMapper VOID_MAPPER = new OutputMapperAnonymousInnerClass();

		 private class OutputMapperAnonymousInnerClass : OutputMapper
		 {
			 public OutputMapperAnonymousInnerClass() : base(new FieldSignature[0], new FieldMapper[0])
			 {
			 }

			 public override IList<FieldSignature> signature()
			 {
				  return ProcedureSignature.VOID;
			 }
		 }

		 /// <summary>
		 /// Extracts field value from an instance and converts it to a Neo4j typed value.
		 /// </summary>
		 private class FieldMapper
		 {
			  internal readonly MethodHandle Getter;
			  internal readonly TypeMappers.TypeChecker Checker;

			  internal FieldMapper( MethodHandle getter, TypeMappers.TypeChecker checker )
			  {
					this.Getter = getter;
					this.Checker = checker;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object apply(Object record) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  internal virtual object Apply( object record )
			  {
					object invoke = GetValue( record );
					return Checker.typeCheck( invoke );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Object getValue(Object record) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  internal virtual object GetValue( object record )
			  {
					try
					{
						 return Getter.invoke( record );
					}
					catch ( Exception throwable )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, throwable, "Unable to read value from record `%s`: %s", record, throwable.Message );
					}
			  }
		 }

		 private readonly Lookup _lookup = MethodHandles.lookup();
		 private readonly TypeMappers _typeMappers;

		 /// <summary>
		 /// Build an output mapper for the return type of a given method.
		 /// </summary>
		 /// <param name="method"> the procedure method </param>
		 /// <returns> an output mapper for the return type of the method. </returns>
		 /// <exception cref="ProcedureException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public OutputMapper mapper(Method method) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual OutputMapper Mapper( System.Reflection.MethodInfo method )
		 {
			  Type cls = method.ReturnType;
			  if ( cls == typeof( Void ) || cls == typeof( void ) )
			  {
					return OutputMappers.VOID_MAPPER;
			  }

			  if ( cls != typeof( Stream ) )
			  {
					throw InvalidReturnType( cls );
			  }

			  Type genericReturnType = method.GenericReturnType;
			  if ( !( genericReturnType is ParameterizedType ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.TypeError, "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define and not a raw Stream." );
			  }

			  ParameterizedType genType = ( ParameterizedType ) genericReturnType;
			  Type recordType = genType.ActualTypeArguments[0];
			  if ( recordType is WildcardType )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.TypeError, "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define and not a Stream<?>." );
			  }
			  if ( recordType is ParameterizedType )
			  {
					ParameterizedType type = ( ParameterizedType ) recordType;
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.TypeError, "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define and not a parameterized type such as %s.", type );
			  }

			  return Mapper( ( Type ) recordType );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public OutputMapper mapper(Class userClass) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual OutputMapper Mapper( Type userClass )
		 {
			  AssertIsValidRecordClass( userClass );

			  IList<System.Reflection.FieldInfo> fields = InstanceFields( userClass );
			  FieldSignature[] signature = new FieldSignature[fields.Count];
			  FieldMapper[] fieldMappers = new FieldMapper[fields.Count];

			  for ( int i = 0; i < fields.Count; i++ )
			  {
					System.Reflection.FieldInfo field = fields[i];
					if ( !isPublic( field.Modifiers ) )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.TypeError, "Field `%s` in record `%s` cannot be accessed. Please ensure the field is marked as `public`.", field.Name, userClass.Name );
					}

					try
					{
						 TypeMappers.TypeChecker checker = _typeMappers.checkerFor( field.GenericType );
						 MethodHandle getter = _lookup.unreflectGetter( field );
						 FieldMapper fieldMapper = new FieldMapper( getter, checker );

						 fieldMappers[i] = fieldMapper;
						 signature[i] = FieldSignature.outputField( field.Name, checker.Type(), field.isAnnotationPresent(typeof(Deprecated)) );
					}
					catch ( ProcedureException e )
					{
						 throw new ProcedureException( e.Status(), e, "Field `%s` in record `%s` cannot be converted to a Neo4j type: %s", field.Name, userClass.Name, e.Message );
					}
					catch ( IllegalAccessException e )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.TypeError, e, "Field `%s` in record `%s` cannot be accessed: %s", field.Name, userClass.Name, e.Message );
					}
			  }

			  return new OutputMapper( signature, fieldMappers );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertIsValidRecordClass(Class userClass) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private void AssertIsValidRecordClass( Type userClass )
		 {
			  if ( userClass.IsPrimitive || userClass.IsArray || userClass.Assembly != null && userClass.Assembly.GetName().Name.StartsWith("java.") )
			  {
					throw InvalidReturnType( userClass );
			  }
		 }

		 private ProcedureException InvalidReturnType( Type userClass )
		 {
			  return new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.TypeError, "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define, with public non-final fields defining the fields in the record.%n" + "If you''d like your procedure to return `%s`, you could define a record class like:%n" + "public class Output '{'%n" + "    public %s out;%n" + "'}'%n" + "%n" + "And then define your procedure as returning `Stream<Output>`.", userClass.Name, userClass.Name );
		 }

		 private IList<System.Reflection.FieldInfo> InstanceFields( Type userClass )
		 {
			  return java.util.userClass.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ).Where( f => !isStatic( f.Modifiers ) && !f.Synthetic ).ToList();
		 }
	}

}