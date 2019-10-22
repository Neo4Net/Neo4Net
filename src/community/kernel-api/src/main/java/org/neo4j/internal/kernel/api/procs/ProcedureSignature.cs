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
	using Mode = Neo4Net.Procedure.Mode;

	/// <summary>
	/// This describes the signature of a procedure, made up of its namespace, name, and input/output description.
	/// Procedure uniqueness is currently *only* on the namespace/name level - no procedure overloading allowed (yet).
	/// </summary>
	public class ProcedureSignature
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public static readonly IList<FieldSignature> VoidConflict = unmodifiableList( new List<FieldSignature>() );

		 private readonly QualifiedName _name;
		 private readonly IList<FieldSignature> _inputSignature;
		 private readonly IList<FieldSignature> _outputSignature;
		 private readonly Mode _mode;
		 private readonly bool _admin;
		 private readonly string _deprecated;
		 private readonly string[] _allowed;
		 private readonly string _description;
		 private readonly string _warning;
		 private readonly bool _eager;
		 private readonly bool _caseInsensitive;

		 public ProcedureSignature( QualifiedName name, IList<FieldSignature> inputSignature, IList<FieldSignature> outputSignature, Mode mode, bool admin, string deprecated, string[] allowed, string description, string warning, bool eager, bool caseInsensitive )
		 {
			  this._name = name;
			  this._inputSignature = unmodifiableList( inputSignature );
			  this._outputSignature = outputSignature == VoidConflict ? outputSignature : unmodifiableList( outputSignature );
			  this._mode = mode;
			  this._admin = admin;
			  this._deprecated = deprecated;
			  this._allowed = allowed;
			  this._description = description;
			  this._warning = warning;
			  this._eager = eager;
			  this._caseInsensitive = caseInsensitive;
		 }

		 public virtual QualifiedName Name()
		 {
			  return _name;
		 }

		 public virtual Mode Mode()
		 {
			  return _mode;
		 }

		 public virtual bool Admin()
		 {
			  return _admin;
		 }

		 public virtual Optional<string> Deprecated()
		 {
			  return Optional.ofNullable( _deprecated );
		 }

		 public virtual string[] Allowed()
		 {
			  return _allowed;
		 }

		 public virtual bool CaseInsensitive()
		 {
			  return _caseInsensitive;
		 }

		 public virtual IList<FieldSignature> InputSignature()
		 {
			  return _inputSignature;
		 }

		 public virtual IList<FieldSignature> OutputSignature()
		 {
			  return _outputSignature;
		 }

		 public virtual bool Void
		 {
			 get
			 {
				  return _outputSignature == VoidConflict;
			 }
		 }

		 public virtual Optional<string> Description()
		 {
			  return Optional.ofNullable( _description );
		 }

		 public virtual Optional<string> Warning()
		 {
			  return Optional.ofNullable( _warning );
		 }

		 public virtual bool Eager()
		 {
			  return _eager;
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

			  ProcedureSignature that = ( ProcedureSignature ) o;
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: return name.equals(that.name) && inputSignature.equals(that.inputSignature) && outputSignature.equals(that.outputSignature) && isVoid() == that.isVoid();
			  return _name.Equals( that._name ) && _inputSignature.SequenceEqual( that._inputSignature ) && _outputSignature.SequenceEqual( that._outputSignature ) && Void == that.Void;
		 }

		 public override int GetHashCode()
		 {
			  return _name.GetHashCode();
		 }

		 public override string ToString()
		 {
			  string strInSig = _inputSignature == null ? "..." : Iterables.ToString( _inputSignature, ", " );
			  if ( Void )
			  {
					return string.Format( "{0}({1}) :: VOID", _name, strInSig );
			  }
			  else
			  {
					string strOutSig = _outputSignature == null ? "..." : Iterables.ToString( _outputSignature, ", " );
					return string.Format( "{0}({1}) :: ({2})", _name, strInSig, strOutSig );
			  }
		 }

		 public class Builder
		 {
			  internal readonly QualifiedName Name;
			  internal readonly IList<FieldSignature> InputSignature = new LinkedList<FieldSignature>();
			  internal IList<FieldSignature> OutputSignature = new LinkedList<FieldSignature>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Mode ModeConflict = Mode.READ;
			  internal string Deprecated;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string[] AllowedConflict = new string[0];
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string DescriptionConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string WarningConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool EagerConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool AdminConflict;

			  public Builder( string[] @namespace, string name )
			  {
					this.Name = new QualifiedName( @namespace, name );
			  }

			  public virtual Builder Mode( Mode mode )
			  {
					this.ModeConflict = mode;
					return this;
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
			  public virtual Builder In( string name, Neo4NetTypes.AnyType type )
			  {
					InputSignature.Add( FieldSignature.InputField( name, type ) );
					return this;
			  }

			  /// <summary>
			  /// Define an output field </summary>
			  public virtual Builder Out( string name, Neo4NetTypes.AnyType type )
			  {
					OutputSignature.Add( FieldSignature.OutputField( name, type ) );
					return this;
			  }

			  public virtual Builder Out( IList<FieldSignature> fields )
			  {
					OutputSignature = fields;
					return this;
			  }

			  public virtual Builder Allowed( string[] allowed )
			  {
					this.AllowedConflict = allowed;
					return this;
			  }

			  public virtual Builder Admin( bool admin )
			  {
					this.AdminConflict = admin;
					return this;
			  }

			  public virtual Builder Warning( string warning )
			  {
					this.WarningConflict = warning;
					return this;
			  }

			  public virtual Builder Eager( bool eager )
			  {
					this.EagerConflict = eager;
					return this;
			  }

			  public virtual ProcedureSignature Build()
			  {
					return new ProcedureSignature( Name, InputSignature, OutputSignature, ModeConflict, AdminConflict, Deprecated, AllowedConflict, DescriptionConflict, WarningConflict, EagerConflict, false );
			  }
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Builder ProcedureSignatureConflict( params string[] namespaceAndName )
		 {
			  string[] @namespace = namespaceAndName.Length > 1 ? Arrays.copyOf( namespaceAndName, namespaceAndName.Length - 1 ) : new string[0];
			  string name = namespaceAndName[namespaceAndName.Length - 1];
			  return ProcedureSignatureConflict( @namespace, name );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Builder ProcedureSignatureConflict( QualifiedName name )
		 {
			  return new Builder( name.Namespace(), name.Name() );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Builder ProcedureSignatureConflict( string[] @namespace, string name )
		 {
			  return new Builder( @namespace, name );
		 }

		 public static QualifiedName ProcedureName( params string[] namespaceAndName )
		 {
			  return ProcedureSignatureConflict( namespaceAndName ).build().Name();
		 }
	}

}