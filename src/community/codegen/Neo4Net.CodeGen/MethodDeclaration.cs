using System;
using System.Collections.Generic;

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
namespace Neo4Net.CodeGen
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Parameter.param;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.NO_TYPES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.typeReference;

	public abstract class MethodDeclaration
	{
		 public static Builder Method( Type returnType, string name, params Parameter[] parameters )
		 {
			  return Method( typeReference( returnType ), name, parameters );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Builder method(final TypeReference returnType, final String name, Parameter... parameters)
		 public static Builder Method( TypeReference returnType, string name, params Parameter[] parameters )
		 {
			  return new BuilderAnonymousInnerClass( parameters, returnType, name );
		 }

		 private class BuilderAnonymousInnerClass : Builder
		 {
			 private Neo4Net.CodeGen.TypeReference _returnType;
			 private string _name;
			 private new Neo4Net.CodeGen.Parameter[] _parameters;

			 public BuilderAnonymousInnerClass( Neo4Net.CodeGen.Parameter[] parameters, Neo4Net.CodeGen.TypeReference returnType, string name ) : base( parameters )
			 {
				 this._returnType = returnType;
				 this._name = name;
				 this._parameters = parameters;
			 }

			 internal override MethodDeclaration build( TypeReference owner )
			 {
				  return Method( owner, _returnType, _name, _parameters, exceptions(), outerInstance.modifiers(), outerInstance.typeParameters() );
			 }
		 }

		 internal static Builder Constructor( params Parameter[] parameters )
		 {
			  return new BuilderAnonymousInnerClass2( parameters );
		 }

		 private class BuilderAnonymousInnerClass2 : Builder
		 {
			 private new Neo4Net.CodeGen.Parameter[] _parameters;

			 public BuilderAnonymousInnerClass2( Neo4Net.CodeGen.Parameter[] parameters ) : base( parameters )
			 {
				 this._parameters = parameters;
			 }

			 internal override MethodDeclaration build( TypeReference owner )
			 {
				  return Constructor( owner, _parameters, exceptions(), outerInstance.modifiers(), outerInstance.typeParameters() );
			 }
		 }

		 public virtual IList<TypeParameter> TypeParameters()
		 {
			  return unmodifiableList( asList( _typeParameters ) );
		 }

		 public virtual IList<TypeReference> ThrowsList()
		 {
			  return unmodifiableList( asList( _exceptions ) );
		 }

		 public abstract class Builder
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal LinkedHashMap<string, TypeReference.Bound> TypeParametersConflict;

			  public virtual Builder ParameterizedWith( string name, TypeReference.Bound bound )
			  {
					if ( TypeParametersConflict == null )
					{
						 TypeParametersConflict = new LinkedHashMap<string, TypeReference.Bound>();
					}
					else if ( TypeParametersConflict.containsKey( name ) )
					{
						 throw new System.ArgumentException( name + " defined twice" );
					}
					TypeParametersConflict.put( name, bound );
					return this;
			  }

			  public virtual Builder ThrowsException( Type type )
			  {
					return ThrowsException( TypeReference.TypeReferenceConflict( type ) );
			  }

			  public virtual Builder ThrowsException( TypeReference type )
			  {
					if ( ExceptionsConflict == null )
					{
						 ExceptionsConflict = new List<TypeReference>();
					}
					ExceptionsConflict.Add( type );
					return this;
			  }

			  public virtual Builder Modifiers( int modifiers )
			  {
					this.ModifiersConflict = modifiers;
					return this;
			  }

			  public virtual int Modifiers()
			  {
					return ModifiersConflict;
			  }

			  internal abstract MethodDeclaration Build( TypeReference owner );

			  internal readonly Parameter[] Parameters;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IList<TypeReference> ExceptionsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int ModifiersConflict = Modifier.PUBLIC;

			  internal Builder( Parameter[] parameters )
			  {
					this.Parameters = parameters;
			  }

			  internal virtual TypeReference[] Exceptions()
			  {
					return ExceptionsConflict == null ? NO_TYPES : ExceptionsConflict.ToArray();
			  }

			  internal virtual TypeParameter[] TypeParameters()
			  {
					if ( TypeParametersConflict == null )
					{
						 return TypeParameter.NoParameters;
					}
					else
					{
						 TypeParameter[] result = new TypeParameter[TypeParametersConflict.size()];
						 int i = 0;
						 foreach ( KeyValuePair<string, TypeReference.Bound> entry in TypeParametersConflict.entrySet() )
						 {
							  result[i++] = new TypeParameter( entry.Key, entry.Value );
						 }
						 return result;
					}
			  }
		 }

		 private readonly TypeReference _owner;
		 private readonly Parameter[] _parameters;
		 private readonly TypeReference[] _exceptions;
		 private readonly TypeParameter[] _typeParameters;
		 private readonly int _modifiers;

		 internal MethodDeclaration( TypeReference owner, Parameter[] parameters, TypeReference[] exceptions, int modifiers, TypeParameter[] typeParameters )
		 {
			  this._owner = owner;
			  this._parameters = parameters;
			  this._exceptions = exceptions;
			  this._modifiers = modifiers;
			  this._typeParameters = typeParameters;
		 }

		 public abstract bool Constructor { get; }

		 public virtual bool Static
		 {
			 get
			 {
				  return Modifier.isStatic( _modifiers );
			 }
		 }

		 public virtual bool Generic
		 {
			 get
			 {
				  if ( ReturnType().Generic || _typeParameters.Length != 0 )
				  {
						return true;
				  }
				  foreach ( Parameter parameter in _parameters )
				  {
						if ( parameter.Type().Generic )
						{
							 return true;
						}
				  }
   
				  return false;
			 }
		 }

		 public virtual TypeReference DeclaringClass()
		 {
			  return _owner;
		 }

		 public virtual int Modifiers()
		 {
			  return _modifiers;
		 }

		 public abstract TypeReference ReturnType();

		 public abstract string Name();

		 public virtual Parameter[] Parameters()
		 {
			  return _parameters;
		 }

		 public virtual MethodDeclaration Erased()
		 {
			  IDictionary<string, TypeReference> table = new Dictionary<string, TypeReference>();
			  foreach ( TypeParameter parameter in _typeParameters )
			  {
					table[parameter.Name()] = parameter.ExtendsBound();
			  }

			  TypeReference newReturnType = Erase( ReturnType(), table );
			  Parameter[] newParameters = new Parameter[this._parameters.Length];
			  for ( int i = 0; i < _parameters.Length; i++ )
			  {
					Parameter parameter = _parameters[i];
					TypeReference erasedType = Erase( parameter.Type(), table );
					newParameters[i] = param( erasedType, parameter.Name() );
			  }
			  TypeReference[] newExceptions = new TypeReference[_exceptions.Length];
			  for ( int i = 0; i < _exceptions.Length; i++ )
			  {
					newExceptions[i] = Erase( _exceptions[i], table );
			  }
			  string newName = Name();
			  bool newIsConstructor = System.Reflection.ConstructorInfo;

			  return MethodDeclarationConflict( _owner, newReturnType, newParameters, newExceptions, newName, newIsConstructor, _modifiers, _typeParameters );
		 }

		 private TypeReference Erase( TypeReference reference, IDictionary<string, TypeReference> table )
		 {
			  TypeReference erasedReference = table[reference.FullName()];

			  return erasedReference != null ? erasedReference : reference;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static MethodDeclaration method(TypeReference owner, final TypeReference returnType, final String name, Parameter[] parameters, TypeReference[] exceptions, int modifiers, TypeParameter[] typeParameters)
		 internal static MethodDeclaration Method( TypeReference owner, TypeReference returnType, string name, Parameter[] parameters, TypeReference[] exceptions, int modifiers, TypeParameter[] typeParameters )
		 {
			  return MethodDeclarationConflict( owner, returnType, parameters, exceptions, name, false, modifiers, typeParameters );
		 }

		 internal static MethodDeclaration Constructor( TypeReference owner, Parameter[] parameters, TypeReference[] exceptions, int modifiers, TypeParameter[] typeParameters )
		 {
			  return MethodDeclarationConflict( owner, TypeReference.VoidConflict, parameters, exceptions, "<init>", true, modifiers, typeParameters );
		 }

		 public class TypeParameter
		 {
			  internal static readonly TypeParameter[] NoParameters = new TypeParameter[] {};

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string NameConflict;
			  internal readonly TypeReference.Bound Bound;

			  internal TypeParameter( string name, TypeReference.Bound bound )
			  {
					this.NameConflict = name;
					this.Bound = bound;
			  }

			  public virtual string Name()
			  {
					return NameConflict;
			  }

			  public virtual TypeReference ExtendsBound()
			  {
					return Bound.extendsBound();
			  }

			  public virtual TypeReference SuperBound()
			  {
					return Bound.superBound();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static MethodDeclaration methodDeclaration(TypeReference owner, final TypeReference returnType, final Parameter[] parameters, final TypeReference[] exceptions, final String name, final boolean isConstructor, int modifiers, TypeParameter[] typeParameters)
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 private static MethodDeclaration MethodDeclarationConflict( TypeReference owner, TypeReference returnType, Parameter[] parameters, TypeReference[] exceptions, string name, bool isConstructor, int modifiers, TypeParameter[] typeParameters )
		 {
			  return new MethodDeclarationAnonymousInnerClass( owner, parameters, exceptions, modifiers, typeParameters, returnType, name, isConstructor );
		 }

		 private class MethodDeclarationAnonymousInnerClass : MethodDeclaration
		 {
			 private Neo4Net.CodeGen.TypeReference _returnType;
			 private string _name;
			 private bool _isConstructor;

			 public MethodDeclarationAnonymousInnerClass( Neo4Net.CodeGen.TypeReference owner, Neo4Net.CodeGen.Parameter[] parameters, Neo4Net.CodeGen.TypeReference[] exceptions, int modifiers, Neo4Net.CodeGen.MethodDeclaration.TypeParameter[] typeParameters, Neo4Net.CodeGen.TypeReference returnType, string name, bool isConstructor ) : base( owner, parameters, exceptions, modifiers, typeParameters )
			 {
				 this._returnType = returnType;
				 this._name = name;
				 this._isConstructor = isConstructor;
			 }

			 public override bool Constructor
			 {
				 get
				 {
					  return _isConstructor;
				 }
			 }

			 public override TypeReference returnType()
			 {
				  return _returnType;
			 }

			 public override string name()
			 {
				  return _name;
			 }
		 }
	}

}