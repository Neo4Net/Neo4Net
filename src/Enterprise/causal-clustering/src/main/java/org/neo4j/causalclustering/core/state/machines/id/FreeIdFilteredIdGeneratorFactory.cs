using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.state.machines.id
{

	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;

	public class FreeIdFilteredIdGeneratorFactory : IdGeneratorFactory
	{
		 private IDictionary<IdType, IdGenerator> _delegatedGenerator = new Dictionary<IdType, IdGenerator>();
		 private readonly IdGeneratorFactory @delegate;
		 private readonly System.Func<bool> _freeIdCondition;

		 public FreeIdFilteredIdGeneratorFactory( IdGeneratorFactory @delegate, System.Func<bool> freeIdCondition )
		 {
			  this.@delegate = @delegate;
			  this._freeIdCondition = freeIdCondition;
		 }

		 public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
		 {
			  FreeIdFilteredIdGenerator freeIdFilteredIdGenerator = new FreeIdFilteredIdGenerator( @delegate.Open( filename, idType, highId, maxId ), _freeIdCondition );
			  _delegatedGenerator[idType] = freeIdFilteredIdGenerator;
			  return freeIdFilteredIdGenerator;
		 }

		 public override IdGenerator Open( File filename, int grabSize, IdType idType, System.Func<long> highId, long maxId )
		 {
			  FreeIdFilteredIdGenerator freeIdFilteredIdGenerator = new FreeIdFilteredIdGenerator( @delegate.Open( filename, grabSize, idType, highId, maxId ), _freeIdCondition );
			  _delegatedGenerator[idType] = freeIdFilteredIdGenerator;
			  return freeIdFilteredIdGenerator;
		 }

		 public override void Create( File filename, long highId, bool throwIfFileExists )
		 {
			  @delegate.Create( filename, highId, throwIfFileExists );
		 }

		 public override IdGenerator Get( IdType idType )
		 {
			  return _delegatedGenerator[idType];
		 }
	}

}