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
namespace Neo4Net.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using SchemaKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.schema.SchemaUtil;

	public abstract class RepeatedSchemaComponentException : SchemaKernelException
	{
		 private readonly SchemaDescriptor _schema;
		 private readonly OperationContext _context;
		 private readonly SchemaComponent _component;

		 internal RepeatedSchemaComponentException( Status status, SchemaDescriptor schema, OperationContext context, SchemaComponent component ) : base( status, Format( schema, context, SchemaUtil.idTokenNameLookup, component ) )
		 {
			  this._schema = schema;
			  this._context = context;
			  this._component = component;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return Format( _schema, _context, tokenNameLookup, _component );
		 }

		 internal sealed class SchemaComponent
		 {
			  public static readonly SchemaComponent Property = new SchemaComponent( "Property", InnerEnum.Property, "property" );
			  public static readonly SchemaComponent Label = new SchemaComponent( "Label", InnerEnum.Label, "label" );
			  public static readonly SchemaComponent RelationshipType = new SchemaComponent( "RelationshipType", InnerEnum.RelationshipType, "relationship type" );

			  private static readonly IList<SchemaComponent> valueList = new List<SchemaComponent>();

			  static SchemaComponent()
			  {
				  valueList.Add( Property );
				  valueList.Add( Label );
				  valueList.Add( RelationshipType );
			  }

			  public enum InnerEnum
			  {
				  Property,
				  Label,
				  RelationshipType
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal SchemaComponent( string name, InnerEnum innerEnum, string name )
			  {
					this._name = name;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<SchemaComponent> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static SchemaComponent ValueOf( string name )
			 {
				 foreach ( SchemaComponent enumInstance in SchemaComponent.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private static string Format( SchemaDescriptor schema, OperationContext context, TokenNameLookup tokenNameLookup, SchemaComponent component )
		 {
			  string schemaName;
			  switch ( context )
			  {
			  case SchemaKernelException.OperationContext.INDEX_CREATION:
					schemaName = "Index";
					break;

			  case SchemaKernelException.OperationContext.CONSTRAINT_CREATION:
					schemaName = "Constraint";
					break;

			  default:
					schemaName = "Schema object";
					break;
			  }
			  return string.Format( "{0} on {1} includes a {2} more than once.", schemaName, Schema.userDescription( tokenNameLookup ), component.name );

		 }
	}

}