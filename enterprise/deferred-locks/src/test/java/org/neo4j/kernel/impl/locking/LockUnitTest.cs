using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Kernel.impl.locking
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class LockUnitTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveLocksAppearFirst()
		 public virtual void ExclusiveLocksAppearFirst()
		 {
			  LockUnit unit1 = new LockUnit( ResourceTypes.Node, 1, true );
			  LockUnit unit2 = new LockUnit( ResourceTypes.Node, 2, false );
			  LockUnit unit3 = new LockUnit( ResourceTypes.Relationship, 1, false );
			  LockUnit unit4 = new LockUnit( ResourceTypes.Relationship, 2, true );
			  LockUnit unit5 = new LockUnit( ResourceTypes.RelationshipType, 1, false );

			  IList<LockUnit> list = new IList<LockUnit> { unit1, unit2, unit3, unit4, unit5 };
			  list.Sort();

			  assertEquals( asList( unit1, unit4, unit2, unit3, unit5 ), list );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exclusiveOrderedByResourceTypes()
		 public virtual void ExclusiveOrderedByResourceTypes()
		 {
			  LockUnit unit1 = new LockUnit( ResourceTypes.Node, 1, true );
			  LockUnit unit2 = new LockUnit( ResourceTypes.Relationship, 1, true );
			  LockUnit unit3 = new LockUnit( ResourceTypes.Node, 2, true );
			  LockUnit unit4 = new LockUnit( ResourceTypes.RelationshipType, 1, true );
			  LockUnit unit5 = new LockUnit( ResourceTypes.Relationship, 2, true );

			  IList<LockUnit> list = new IList<LockUnit> { unit1, unit2, unit3, unit4, unit5 };
			  list.Sort();

			  assertEquals( asList( unit1, unit3, unit2, unit5, unit4 ), list );
		 }
	}

}