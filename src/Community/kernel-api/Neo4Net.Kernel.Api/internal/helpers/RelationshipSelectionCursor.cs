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
namespace Neo4Net.Kernel.Api.Internal.Helpers
{
	/// <summary>
	/// Helper ICursor for traversing specific types and directions.
	/// </summary>
	public interface RelationshipSelectionCursor : IDisposable
	{
		 bool Next();

		 void Close();

		 long RelationshipReference();

		 int Type();

		 long OtherNodeReference();

		 long SourceNodeReference();

		 long TargetNodeReference();

		 long PropertiesReference();
	}

	public static class RelationshipSelectionCursor_Fields
	{
		 public static readonly RelationshipSelectionCursor Empty = new Empty();
	}

	 public sealed class RelationshipSelectionCursor_EMPTY : RelationshipSelectionCursor
	 {
		  public override bool Next()
		  {
				return false;
		  }

		  public override void Close()
		  {

		  }

		  public override long RelationshipReference()
		  {
				return -1;
		  }

		  public override int Type()
		  {
				return -1;
		  }

		  public override long OtherNodeReference()
		  {
				return -1;
		  }

		  public override long SourceNodeReference()
		  {
				return -1;
		  }

		  public override long TargetNodeReference()
		  {
				return -1;
		  }

		  public override long PropertiesReference()
		  {
				return -1;
		  }
	 }

}