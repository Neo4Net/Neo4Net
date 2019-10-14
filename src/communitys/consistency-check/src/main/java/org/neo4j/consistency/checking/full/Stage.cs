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
namespace Neo4Net.Consistency.checking.full
{
	/// <summary>
	/// Represents a <seealso cref="Stage"/> in the consistency check. A consistency check goes through multiple stages.
	/// </summary>
	public interface Stage
	{
		 bool Parallel { get; }

		 bool Forward { get; }

		 string Purpose { get; }

		 int Ordinal();

		 int[] CacheSlotSizes { get; }
	}

	public static class Stage_Fields
	{
		 public static readonly Stage SequentialForward = new Adapter( false, true, "General purpose" );
		 public static readonly Stage ParallelForward = new Adapter( true, true, "General purpose" );
	}

	 public class Stage_Adapter : Stage
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly bool ParallelConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly bool ForwardConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly string PurposeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly int[] CacheSlotSizesConflict;

		  public Stage_Adapter( bool parallel, bool forward, string purpose, params int[] cacheSlotSizes )
		  {
				this.ParallelConflict = parallel;
				this.ForwardConflict = forward;
				this.PurposeConflict = purpose;
				this.CacheSlotSizesConflict = cacheSlotSizes;
		  }

		  public virtual bool Parallel
		  {
			  get
			  {
					return ParallelConflict;
			  }
		  }

		  public virtual bool Forward
		  {
			  get
			  {
					return ForwardConflict;
			  }
		  }

		  public virtual string Purpose
		  {
			  get
			  {
					return PurposeConflict;
			  }
		  }

		  public override int Ordinal()
		  {
				return -1;
		  }

		  public virtual int[] CacheSlotSizes
		  {
			  get
			  {
					return CacheSlotSizesConflict;
			  }
		  }
	 }

}