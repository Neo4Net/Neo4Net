using System.Text;

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
namespace Neo4Net.Kernel.Api.Internal.Procs
{

	using AnyValue = Neo4Net.Values.AnyValue;
	using Neo4Net.Values;

	/// <summary>
	/// Represents a type and a name for a field in a record, used to define input and output record signatures. </summary>
	public class FieldSignature
	{
		 public static FieldSignature InputField( string name, Neo4NetTypes.AnyType type )
		 {
			  return new FieldSignature( name, type, null, false );
		 }

		 public static FieldSignature InputField( string name, Neo4NetTypes.AnyType type, DefaultParameterValue defaultValue )
		 {
			  return new FieldSignature( name, type, requireNonNull( defaultValue, "defaultValue" ), false );
		 }

		 public interface IInputMapper
		 {
			  object Map( object input );
			  AnyValue Map( AnyValue input );
		 }

		 public static FieldSignature InputField( string name, Neo4NetTypes.AnyType type, InputMapper mapper )
		 {
			  return new FieldSignatureAnonymousInnerClass( name, type, mapper );
		 }

		 private class FieldSignatureAnonymousInnerClass : FieldSignature
		 {
			 private Neo4Net.Kernel.Api.Internal.Procs.FieldSignature.InputMapper _mapper;

			 public FieldSignatureAnonymousInnerClass( string name, Neo4Net.Kernel.Api.Internal.Procs.Neo4NetTypes.AnyType type, Neo4Net.Kernel.Api.Internal.Procs.FieldSignature.InputMapper mapper ) : base( name, type, null, false )
			 {
				 this._mapper = mapper;
			 }

			 public override object map( object input )
			 {
				  return _mapper.map( input );
			 }

			 public override object map( AnyValue input, IValueMapper<object> valueMapper )
			 {
				  return _mapper.map( input ).map( valueMapper );
			 }

			 public override bool needsMapping()
			 {
				  return true;
			 }
		 }

		 public static FieldSignature InputField( string name, Neo4NetTypes.AnyType type, DefaultParameterValue defaultValue, InputMapper mapper )
		 {
			  return new FieldSignatureAnonymousInnerClass2( name, type, requireNonNull( defaultValue, "defaultValue" ), mapper );
		 }

		 private class FieldSignatureAnonymousInnerClass2 : FieldSignature
		 {
			 private Neo4Net.Kernel.Api.Internal.Procs.FieldSignature.InputMapper _mapper;

			 public FieldSignatureAnonymousInnerClass2( string name, Neo4Net.Kernel.Api.Internal.Procs.Neo4NetTypes.AnyType type, UnknownType requireNonNull, Neo4Net.Kernel.Api.Internal.Procs.FieldSignature.InputMapper mapper ) : base( name, type, requireNonNull, false )
			 {
				 this._mapper = mapper;
			 }

			 public override object map( object input )
			 {
				  return _mapper.map( input );
			 }

			 public override object map( AnyValue input, IValueMapper<object> valueMapper )
			 {
				  return _mapper.map( input ).map( valueMapper );
			 }

			 public override bool needsMapping()
			 {
				  return true;
			 }
		 }

		 public static FieldSignature OutputField( string name, Neo4NetTypes.AnyType type )
		 {
			  return OutputField( name, type, false );
		 }

		 public static FieldSignature OutputField( string name, Neo4NetTypes.AnyType type, bool deprecated )
		 {
			  return new FieldSignature( name, type, null, deprecated );
		 }

		 private readonly string _name;
		 private readonly Neo4NetTypes.AnyType _type;
		 private readonly DefaultParameterValue _defaultValue;
		 private readonly bool _deprecated;

		 private FieldSignature( string name, Neo4NetTypes.AnyType type, DefaultParameterValue defaultValue, bool deprecated )
		 {
			  this._name = requireNonNull( name, "name" );
			  this._type = requireNonNull( type, "type" );
			  this._defaultValue = defaultValue;
			  this._deprecated = deprecated;
			  if ( defaultValue != null )
			  {
					if ( !type.Equals( defaultValue.Neo4NetType() ) )
					{
						 throw new System.ArgumentException( string.Format( "Default value does not have a valid type, field type was {0}, but value type was {1}.", type.ToString(), defaultValue.Neo4NetType().ToString() ) );
					}
			  }
		 }

		 public virtual bool NeedsMapping()
		 {
			  return false;
		 }

		 /// <summary>
		 /// Fields that are not supported full stack (ie. by Cypher) need to be mapped from Cypher to internal types </summary>
		 public virtual object Map( object input )
		 {
			  return input;
		 }

		 public virtual object Map( AnyValue input, IValueMapper<object> mapper )
		 {
			  return input.Map( mapper );
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual Neo4NetTypes.AnyType Neo4NetType()
		 {
			  return _type;
		 }

		 public virtual Optional<DefaultParameterValue> DefaultValue()
		 {
			  return Optional.ofNullable( _defaultValue );
		 }

		 public virtual bool Deprecated
		 {
			 get
			 {
				  return _deprecated;
			 }
		 }

		 public override string ToString()
		 {
			  StringBuilder result = new StringBuilder();
			  result.Append( _name );
			  if ( _defaultValue != null )
			  {
					result.Append( " = " ).Append( _defaultValue.value() );
			  }
			  return result.Append( " :: " ).Append( _type ).ToString();
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
			  FieldSignature that = ( FieldSignature ) o;
			  return _name.Equals( that._name ) && _type.Equals( that._type ) && Objects.Equals( this._defaultValue, that._defaultValue ) && this._deprecated == that._deprecated;
		 }

		 public override int GetHashCode()
		 {
			  int result = _name.GetHashCode();
			  result = 31 * result + _type.GetHashCode();
			  return result;
		 }
	}

}