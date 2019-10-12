/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.store
{
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;

	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;

	public class RecordStoreUtil
	{
		 public class ReadNodeAnswer : Answer<NodeRecord>
		 {
			  internal readonly bool Dense;
			  internal readonly long NextRel;
			  internal readonly long NextProp;

			  public ReadNodeAnswer( bool dense, long nextRel, long nextProp )
			  {
					this.Dense = dense;
					this.NextRel = nextRel;
					this.NextProp = nextProp;
			  }

			  public override NodeRecord Answer( InvocationOnMock invocation )
			  {
					if ( ( ( Number ) invocation.getArgument( 0 ) ).longValue() == 0L && invocation.getArgument(1) == null && invocation.getArgument(2) == null )
					{
						 return null;
					}

					NodeRecord record = invocation.getArgument( 1 );
					record.Id = ( ( Number ) invocation.getArgument( 0 ) ).longValue();
					record.InUse = true;
					record.Dense = Dense;
					record.NextRel = NextRel;
					record.NextProp = NextProp;
					return record;
			  }
		 }
	}

}