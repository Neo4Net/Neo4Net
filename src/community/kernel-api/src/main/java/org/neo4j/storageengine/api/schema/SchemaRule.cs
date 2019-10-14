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
namespace Neo4Net.Storageengine.Api.schema
{
	using MalformedSchemaRuleException = Neo4Net.Internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using LabelSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using Neo4Net.Internal.Kernel.Api.schema;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaDescriptorSupplier = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptorSupplier;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;

	/// <summary>
	/// Represents a stored schema rule.
	/// </summary>
	public interface SchemaRule : SchemaDescriptorSupplier
	{
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static String nameOrDefault(String name, String defaultName)
	//	 {
	//		  if (name == null)
	//		  {
	//				return defaultName;
	//		  }
	//		  else if (name.isEmpty())
	//		  {
	//				throw new IllegalArgumentException("Schema rule name cannot be the empty string");
	//		  }
	//		  else
	//		  {
	//				int length = name.length();
	//				for (int i = 0; i < length; i++)
	//				{
	//					 char ch = name.charAt(i);
	//					 if (ch == '\0')
	//					 {
	//						  throw new IllegalArgumentException("Illegal schema rule name: '" + name + "'");
	//					 }
	//				}
	//		  }
	//		  return name;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static String checkName(String name)
	//	 {
	//		  if (name == null || name.isEmpty())
	//		  {
	//				throw new IllegalArgumentException("Schema rule name cannot be the empty string");
	//		  }
	//		  else
	//		  {
	//				int length = name.length();
	//				for (int i = 0; i < length; i++)
	//				{
	//					 char ch = name.charAt(i);
	//					 if (ch == '\0')
	//					 {
	//						  throw new IllegalArgumentException("Illegal schema rule name: '" + name + "'");
	//					 }
	//				}
	//		  }
	//		  return name;
	//	 }

		 /// <summary>
		 /// The persistence id for this rule.
		 /// </summary>
		 long Id { get; }

		 /// <returns> The (possibly user supplied) name of this schema rule. </returns>
		 string Name { get; }

		 /// <summary>
		 /// This enum is used for the legacy schema store, and should not be extended. </summary>
		 /// <seealso cref= org.neo4j.kernel.impl.store.record.SchemaRuleSerialization for the new (de)serialisation code instead. </seealso>
	}

	 public sealed class SchemaRule_Kind
	 {
		  public static readonly SchemaRule_Kind IndexRule = new SchemaRule_Kind( "IndexRule", InnerEnum.IndexRule, "Index" );
		  public static readonly SchemaRule_Kind ConstraintIndexRule = new SchemaRule_Kind( "ConstraintIndexRule", InnerEnum.ConstraintIndexRule, "Constraint index" );
		  public static readonly SchemaRule_Kind UniquenessConstraint = new SchemaRule_Kind( "UniquenessConstraint", InnerEnum.UniquenessConstraint, "Uniqueness constraint" );
		  public static readonly SchemaRule_Kind NodePropertyExistenceConstraint = new SchemaRule_Kind( "NodePropertyExistenceConstraint", InnerEnum.NodePropertyExistenceConstraint, "Node property existence constraint" );
		  public static readonly SchemaRule_Kind RelationshipPropertyExistenceConstraint = new SchemaRule_Kind( "RelationshipPropertyExistenceConstraint", InnerEnum.RelationshipPropertyExistenceConstraint, "Relationship property existence constraint" );

		  private static readonly IList<SchemaRule_Kind> valueList = new List<SchemaRule_Kind>();

		  static SchemaRule_Kind()
		  {
			  valueList.Add( IndexRule );
			  valueList.Add( ConstraintIndexRule );
			  valueList.Add( UniquenessConstraint );
			  valueList.Add( NodePropertyExistenceConstraint );
			  valueList.Add( RelationshipPropertyExistenceConstraint );
		  }

		  public enum InnerEnum
		  {
			  IndexRule,
			  ConstraintIndexRule,
			  UniquenessConstraint,
			  NodePropertyExistenceConstraint,
			  RelationshipPropertyExistenceConstraint
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Static readonly;

		  internal Final String;

		  internal SchemaRule_Kind( string name, InnerEnum innerEnum, string userString )
		  {
				this.UserStringConflict = userString;

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public sbyte Id()
		  {
				return ( sbyte )( ordinal() + 1 );
		  }

		  public string UserString()
		  {
				return UserStringConflict;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SchemaRule_Kind forId(byte id) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		  public static SchemaRule_Kind ForId( sbyte id )
		  {
				if ( id >= 1 && id <= All.Length )
				{
					 return values()[id - 1];
				}
				throw new MalformedSchemaRuleException( null, "Unknown kind id %d", id );
		  }

		  public static SchemaRule_Kind Map( IndexDescriptor descriptor )
		  {
				switch ( descriptor.Type() )
				{
				case GENERAL:
					 return INDEX_RULE;
				case UNIQUE:
					 return CONSTRAINT_INDEX_RULE;
				default:
					 throw new System.InvalidOperationException( "Cannot map descriptor type to legacy schema rule: " + descriptor.Type() );
				}
		  }

		  public static SchemaRule_Kind Map( Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor descriptor )
		  {
				switch ( descriptor.Type() )
				{
				case UNIQUE:
					 return UNIQUENESS_CONSTRAINT;
				case EXISTS:
					 return descriptor.Schema().computeWith(existenceKindMapper);
				default:
					 throw new System.InvalidOperationException( "Cannot map descriptor type to legacy schema rule: " + descriptor.Type() );
				}
		  }

		  internal Static org;

		 public static IList<SchemaRule_Kind> values()
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

		 public static SchemaRule_Kind valueOf( string name )
		 {
			 foreach ( SchemaRule_Kind enumInstance in SchemaRule_Kind.valueList )
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