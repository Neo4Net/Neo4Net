﻿/*
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

	/// <summary>
	/// Id generator that will perform filtering of ids to free using supplied condition.
	/// Id will be freed only if condition is true, otherwise it will be ignored.
	/// </summary>
	public class FreeIdFilteredIdGenerator : Neo4Net.Kernel.impl.store.id.IdGenerator_Delegate
	{
		 private readonly System.Func<bool> _freeIdCondition;

		 internal FreeIdFilteredIdGenerator( IdGenerator @delegate, System.Func<bool> freeIdCondition ) : base( @delegate )
		 {
			  this._freeIdCondition = freeIdCondition;
		 }

		 public override void FreeId( long id )
		 {
			  if ( _freeIdCondition.AsBoolean )
			  {
					base.FreeId( id );
			  }
		 }
	}

}