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
namespace Neo4Net.Kernel.impl.util.dbstructure
{

	using Strings = Neo4Net.Helpers.Strings;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.Api.schema.constraints.ConstraintDescriptorFactory;
	using NodeExistenceConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeExistenceConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeKeyConstraintDescriptor;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.RelExistenceConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.UniquenessConstraintDescriptor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor.Type.GENERAL;

	public sealed class DbStructureArgumentFormatter : ArgumentFormatter
	{
		 public static readonly DbStructureArgumentFormatter Instance = new DbStructureArgumentFormatter( "Instance", InnerEnum.Instance );

		 private static readonly IList<DbStructureArgumentFormatter> valueList = new List<DbStructureArgumentFormatter>();

		 static DbStructureArgumentFormatter()
		 {
			 valueList.Add( Instance );
		 }

		 public enum InnerEnum
		 {
			 Instance
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private DbStructureArgumentFormatter( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal Private static;

		 public ICollection<string> Imports()
		 {
			  return _imports;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void formatArgument(Appendable builder, Object arg) throws java.io.IOException
		 public void FormatArgument( Appendable builder, object arg )
		 {
			  if ( arg == null )
			  {
					builder.append( "null" );
			  }
			  else if ( arg is string )
			  {
					builder.append( '"' );
					Strings.escape( builder, arg.ToString() );
					builder.append( '"' );
			  }
			  else if ( arg is long? )
			  {
					builder.append( arg.ToString() );
					builder.append( 'L' );
			  }
			  else if ( arg is int? )
			  {
					builder.append( arg.ToString() );
			  }
			  else if ( arg is double? )
			  {
					double d = ( double? ) arg.Value;
					if ( double.IsNaN( d ) )
					{
						 builder.append( "Double.NaN" );
					}
					else if ( double.IsInfinity( d ) )
					{
						 builder.append( d < 0 ? "Double.NEGATIVE_INFINITY" : "Double.POSITIVE_INFINITY" );
					}
					else
					{
						 builder.append( arg.ToString() );
						 builder.append( 'd' );
					}
			  }
			  else if ( arg is IndexDescriptor )
			  {
					IndexDescriptor descriptor = ( IndexDescriptor ) arg;
					string className = typeof( IndexDescriptorFactory ).Name;
					SchemaDescriptor schema = descriptor.Schema();
					string methodName = descriptor.Type() == GENERAL ? "forSchema" : "uniqueForSchema";
					builder.append( string.Format( "{0}.{1}( ", className, methodName ) );
					FormatArgument( builder, schema );
					builder.append( " )" );
			  }
			  else if ( arg is LabelSchemaDescriptor )
			  {
					LabelSchemaDescriptor descriptor = ( LabelSchemaDescriptor ) arg;
					string className = typeof( SchemaDescriptorFactory ).Name;
					int labelId = descriptor.LabelId;
					builder.append( format( "%s.forLabel( %d, %s )", className, labelId, AsString( descriptor.PropertyIds ) ) );
			  }
			  else if ( arg is UniquenessConstraintDescriptor )
			  {
					UniquenessConstraintDescriptor constraint = ( UniquenessConstraintDescriptor ) arg;
					string className = typeof( ConstraintDescriptorFactory ).Name;
					int labelId = constraint.Schema().LabelId;
					builder.append( format( "%s.uniqueForLabel( %d, %s )", className, labelId, AsString( constraint.Schema().PropertyIds ) ) );
			  }
			  else if ( arg is NodeExistenceConstraintDescriptor )
			  {
					NodeExistenceConstraintDescriptor constraint = ( NodeExistenceConstraintDescriptor ) arg;
					string className = typeof( ConstraintDescriptorFactory ).Name;
					int labelId = constraint.Schema().LabelId;
					builder.append( format( "%s.existsForLabel( %d, %s )", className, labelId, AsString( constraint.Schema().PropertyIds ) ) );
			  }
			  else if ( arg is RelExistenceConstraintDescriptor )
			  {
					RelationTypeSchemaDescriptor descriptor = ( ( RelExistenceConstraintDescriptor ) arg ).schema();
					string className = typeof( ConstraintDescriptorFactory ).Name;
					int relTypeId = descriptor.RelTypeId;
					builder.append( format( "%s.existsForReltype( %d, %s )", className, relTypeId, AsString( descriptor.PropertyIds ) ) );
			  }
			  else if ( arg is NodeKeyConstraintDescriptor )
			  {
					NodeKeyConstraintDescriptor constraint = ( NodeKeyConstraintDescriptor ) arg;
					string className = typeof( ConstraintDescriptorFactory ).Name;
					int labelId = constraint.Schema().LabelId;
					builder.append( format( "%s.nodeKeyForLabel( %d, %s )", className, labelId, AsString( constraint.Schema().PropertyIds ) ) );
			  }
			  else
			  {
					throw new System.ArgumentException( format( "Can't handle argument of type: %s with value: %s", arg.GetType(), arg ) );
			  }
		 }

		 private string AsString( int[] propertyIds )
		 {
			  IList<string> strings = Arrays.stream( propertyIds ).mapToObj( i => "" + i ).collect( Collectors.toList() );
			  return string.join( ", ", strings );
		 }

		public static IList<DbStructureArgumentFormatter> values()
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

		public static DbStructureArgumentFormatter ValueOf( string name )
		{
			foreach ( DbStructureArgumentFormatter enumInstance in DbStructureArgumentFormatter.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}