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
namespace Neo4Net.Kernel.Impl.Api.index
{
	public sealed class IndexUpdateMode
	{
		 /// <summary>
		 /// Used when the db is online
		 /// </summary>
		 public static readonly IndexUpdateMode Online = new IndexUpdateMode( "Online", InnerEnum.Online, false, true );

		 /// <summary>
		 /// Used when flipping from populating to online
		 /// </summary>
		 public static readonly IndexUpdateMode OnlineIdempotent = new IndexUpdateMode( "OnlineIdempotent", InnerEnum.OnlineIdempotent, true, true );

		 /// <summary>
		 /// Used when the db is recovering
		 /// </summary>
		 public static readonly IndexUpdateMode Recovery = new IndexUpdateMode( "Recovery", InnerEnum.Recovery, true, false );

		 private static readonly IList<IndexUpdateMode> valueList = new List<IndexUpdateMode>();

		 static IndexUpdateMode()
		 {
			 valueList.Add( Online );
			 valueList.Add( OnlineIdempotent );
			 valueList.Add( Recovery );
		 }

		 public enum InnerEnum
		 {
			 Online,
			 OnlineIdempotent,
			 Recovery
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;

		 internal IndexUpdateMode( string name, InnerEnum innerEnum, bool idempotency, bool refresh )
		 {
			  this._idempotency = idempotency;
			  this._refresh = refresh;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public bool RequiresIdempotency()
		 {
			  return _idempotency;
		 }

		 public bool RequiresRefresh()
		 {
			  return _refresh;
		 }

		public static IList<IndexUpdateMode> values()
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

		public static IndexUpdateMode ValueOf( string name )
		{
			foreach ( IndexUpdateMode enumInstance in IndexUpdateMode.valueList )
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