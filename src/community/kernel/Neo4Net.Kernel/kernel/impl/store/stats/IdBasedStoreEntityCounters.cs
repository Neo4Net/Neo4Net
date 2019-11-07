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
namespace Neo4Net.Kernel.impl.store.stats
{
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.id.IdType.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.id.IdType.PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.id.IdType.RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.id.IdType.RELATIONSHIP_TYPE_TOKEN;

	public class IdBasedStoreEntityCounters : StoreEntityCounters
	{
		 private readonly IdGeneratorFactory _idGeneratorFactory;

		 public IdBasedStoreEntityCounters( IdGeneratorFactory idGeneratorFactory )
		 {
			  this._idGeneratorFactory = idGeneratorFactory;
		 }

		 public override long Nodes()
		 {
			  return _idGeneratorFactory.get( NODE ).NumberOfIdsInUse;
		 }

		 public override long Relationships()
		 {
			  return _idGeneratorFactory.get( RELATIONSHIP ).NumberOfIdsInUse;
		 }

		 public override long Properties()
		 {
			  return _idGeneratorFactory.get( PROPERTY ).NumberOfIdsInUse;
		 }

		 public override long RelationshipTypes()
		 {
			  return _idGeneratorFactory.get( RELATIONSHIP_TYPE_TOKEN ).NumberOfIdsInUse;
		 }
	}

}