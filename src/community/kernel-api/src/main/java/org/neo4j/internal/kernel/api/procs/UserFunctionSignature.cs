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
namespace Neo4Net.Internal.Kernel.Api.procs
{

	using Iterables = Neo4Net.Helpers.Collections.Iterables;

	/// <summary>
	/// This describes the signature of a function, made up of its namespace, name, and input/output description.
	/// Function uniqueness is currently *only* on the namespace/name level - no function overloading allowed (yet).
	/// </summary>
	public sealed class UserFunctionSignature
	{
		 private readonly QualifiedName _name;
		 private readonly IList<FieldSignature> _inputSignature;
		 private readonly Neo4jTypes.AnyType _type;
		 private readonly string[] _allowed;
		 private readonly string _deprecated;
		 private readonly string _description;
		 private readonly bool _caseInsensitive;

		 public UserFunctionSignature( QualifiedName name, IList<FieldSignature> inputSignature, Neo4jTypes.AnyType type, string deprecated, string[] allowed, string description, bool caseInsensitive )
		 {
			  this._name = name;
			  this._inputSignature = unmodifiableList( inputSignature );
			  this._type = type;
			  this._deprecated = deprecated;
			  this._description = description;
			  this._allowed = allowed;
			  this._caseInsensitive = caseInsensitive;
		 }

		 public QualifiedName Name()
		 {
			  return _name;
		 }

		 public Optional<string> Deprecated()
		 {
			  return Optional.ofNullable( _deprecated );
		 }

		 public IList<FieldSignature> InputSignature()
		 {
			  return _inputSignature;
		 }

		 public Neo4jTypes.AnyType OutputType()
		 {
			  return _type;
		 }

		 public Optional<string> Description()
		 {
			  return Optional.ofNullable( _description );
		 }

		 public string[] Allowed()
		 {
			  return _allowed;
		 }

		 public bool CaseInsensitive()
		 {
			  return _caseInsensitive;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  UserFunctionSignature that = ( UserFunctionSignature ) o;
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: return name.equals(that.name) && inputSignature.equals(that.inputSignature) && type.equals(that.type);
			  return _name.Equals( that._name ) && _inputSignature.SequenceEqual( that._inputSignature ) && _type.Equals( that._type );
		 }

		 public override int GetHashCode()
		 {
			  return _name.GetHashCode();
		 }

		 public override string ToString()
		 {
			  string strInSig = _inputSignature == null ? "..." : Iterables.ToString( _inputSignature, ", " );
					string strOutSig = _type == null ? "..." : _type.ToString();
					return string.Format( "{0}({1}) :: ({2})", _name, strInSig, strOutSig );
		 }

		 public class Builder
		 {
			  internal readonly QualifiedName Name;
			  internal readonly IList<FieldSignature> InputSignature = new LinkedList<FieldSignature>();
			  internal Neo4jTypes.AnyType OutputType;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string[] AllowedConflict = new string[0];
			  internal string Deprecated;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string DescriptionConflict;

			  public Builder( string[] @namespace, string name )
			  {
					this.Name = new QualifiedName( @namespace, name );
			  }

			  public virtual Builder Description( string description )
			  {
					this.DescriptionConflict = description;
					return this;
			  }

			  public virtual Builder DeprecatedBy( string deprecated )
			  {
					this.Deprecated = deprecated;
					return this;
			  }

			  /// <summary>
			  /// Define an input field </summary>
			  public virtual Builder In( string name, Neo4jTypes.AnyType type )
			  {
					InputSignature.Add( FieldSignature.InputField( name, type ) );
					return this;
			  }

			  /// <summary>
			  /// Define an output field </summary>
			  public virtual Builder Out( Neo4jTypes.AnyType type )
			  {
					OutputType = type;
					return this;
			  }

			  public virtual Builder Allowed( string[] allowed )
			  {
					this.AllowedConflict = allowed;
					return this;
			  }

			  public virtual UserFunctionSignature Build()
			  {
					if ( OutputType == null )
					{
						 throw new System.InvalidOperationException( "output type must be set" );
					}
					return new UserFunctionSignature( Name, InputSignature, OutputType, Deprecated, AllowedConflict, DescriptionConflict, false );
			  }
		 }

		 public static Builder FunctionSignature( params string[] namespaceAndName )
		 {
			  string[] @namespace = namespaceAndName.Length > 1 ? Arrays.copyOf( namespaceAndName, namespaceAndName.Length - 1 ) : new string[0];
			  string name = namespaceAndName[namespaceAndName.Length - 1];
			  return FunctionSignature( @namespace, name );
		 }

		 public static Builder FunctionSignature( QualifiedName name )
		 {
			  return new Builder( name.Namespace(), name.Name() );
		 }

		 public static Builder FunctionSignature( string[] @namespace, string name )
		 {
			  return new Builder( @namespace, name );
		 }

		 public static QualifiedName ProcedureName( params string[] namespaceAndName )
		 {
			  return FunctionSignature( namespaceAndName ).build().Name();
		 }
	}

}