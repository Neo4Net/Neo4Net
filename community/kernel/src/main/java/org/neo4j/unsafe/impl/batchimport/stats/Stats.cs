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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.stats
{
	/// <summary>
	/// Common <seealso cref="Stat"/> implementations.
	/// </summary>
	public class Stats
	{
		 public abstract class LongBasedStat : Stat
		 {
			 public abstract long AsLong();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly DetailLevel DetailLevelConflict;

			  public LongBasedStat( DetailLevel detailLevel )
			  {
					this.DetailLevelConflict = detailLevel;
			  }

			  public override DetailLevel DetailLevel()
			  {
					return DetailLevelConflict;
			  }

			  public override string ToString()
			  {
					return AsLong().ToString();
			  }
		 }

		 private Stats()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Stat longStat(final long stat)
		 public static Stat LongStat( long stat )
		 {
			  return LongStat( stat, DetailLevel.Basic );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Stat longStat(final long stat, DetailLevel detailLevel)
		 public static Stat LongStat( long stat, DetailLevel detailLevel )
		 {
			  return new LongBasedStatAnonymousInnerClass( detailLevel, stat );
		 }

		 private class LongBasedStatAnonymousInnerClass : LongBasedStat
		 {
			 private long _stat;

			 public LongBasedStatAnonymousInnerClass( Org.Neo4j.@unsafe.Impl.Batchimport.stats.DetailLevel detailLevel, long stat ) : base( detailLevel )
			 {
				 this._stat = stat;
			 }

			 public override long asLong()
			 {
				  return _stat;
			 }
		 }
	}

}