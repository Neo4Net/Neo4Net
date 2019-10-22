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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;

	/// <summary>
	/// Collects updates from <seealso cref="process(IndexEntryUpdate)"/> and passes them to the <seealso cref="Applier"/> on <seealso cref="close()"/>.
	/// </summary>
	public class CollectingIndexUpdater : IndexUpdater
	{
		 private readonly Applier _applier;

		 private bool _closed;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Collection<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
		 private readonly ICollection<IndexEntryUpdate<object>> _updates = new List<IndexEntryUpdate<object>>();

		 public CollectingIndexUpdater( Applier applier )
		 {
			  this._applier = applier;
		 }

		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  AssertOpen();
			  _updates.Add( update );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Close()
		 {
			  AssertOpen();
			  try
			  {
					_applier.accept( _updates );
			  }
			  finally
			  {
					_closed = true;
			  }
		 }

		 private void AssertOpen()
		 {
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "Updater has been closed" );
			  }
		 }

		 public interface Applier
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void accept(java.util.Collection<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updates) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  void accept<T1>( ICollection<T1> updates );
		 }
	}

}