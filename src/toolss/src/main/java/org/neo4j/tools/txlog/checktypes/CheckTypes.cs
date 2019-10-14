/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.tools.txlog.checktypes
{
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;

	public class CheckTypes
	{
		 public static readonly NodeCheckType Node = new NodeCheckType();
		 public static readonly PropertyCheckType Property = new PropertyCheckType();
		 public static readonly RelationshipCheckType Relationship = new RelationshipCheckType();
		 public static readonly RelationshipGroupCheckType RelationshipGroup = new RelationshipGroupCheckType();
		 public static readonly NeoStoreCheckType NeoStore = new NeoStoreCheckType();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static final CheckType<? extends org.neo4j.kernel.impl.transaction.command.Command, ? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>[] CHECK_TYPES = new CheckType[]{NODE, PROPERTY, RELATIONSHIP, RELATIONSHIP_GROUP, NEO_STORE};
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static readonly CheckType<Command, ? extends AbstractBaseRecord>[] CheckTypesConflict = new CheckType[]{ Node, Property, Relationship, RelationshipGroup, NeoStore };

		 private CheckTypes()
		 {
		 }

		 public static CheckType<C, R> FromName<C, R>( string name ) where C : Neo4Net.Kernel.impl.transaction.command.Command where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (CheckType<?,?> checkType : CHECK_TYPES)
			  foreach ( CheckType<object, ?> checkType in CheckTypesConflict )
			  {
					if ( checkType.Name().Equals(name) )
					{
						 //noinspection unchecked
						 return ( CheckType<C, R> ) checkType;
					}
			  }
			  throw new System.ArgumentException( "Unknown check named " + name );
		 }
	}

}