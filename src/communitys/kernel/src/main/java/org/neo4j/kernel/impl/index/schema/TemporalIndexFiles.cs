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

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	internal class TemporalIndexFiles
	{
		 private readonly FileSystemAbstraction _fs;
		 private FileLayout<DateIndexKey> _date;
		 private FileLayout<LocalDateTimeIndexKey> _localDateTime;
		 private FileLayout<ZonedDateTimeIndexKey> _zonedDateTime;
		 private FileLayout<LocalTimeIndexKey> _localTime;
		 private FileLayout<ZonedTimeIndexKey> _zonedTime;
		 private FileLayout<DurationIndexKey> _duration;

		 internal TemporalIndexFiles( IndexDirectoryStructure directoryStructure, StoreIndexDescriptor descriptor, FileSystemAbstraction fs )
		 {
			  this._fs = fs;
			  File indexDirectory = directoryStructure.DirectoryForIndex( descriptor.Id );
			  this._date = new FileLayout<DateIndexKey>( new File( indexDirectory, "date" ), new DateLayout(), ValueGroup.DATE );
			  this._localTime = new FileLayout<LocalTimeIndexKey>( new File( indexDirectory, "localTime" ), new LocalTimeLayout(), ValueGroup.LOCAL_TIME );
			  this._zonedTime = new FileLayout<ZonedTimeIndexKey>( new File( indexDirectory, "zonedTime" ), new ZonedTimeLayout(), ValueGroup.ZONED_TIME );
			  this._localDateTime = new FileLayout<LocalDateTimeIndexKey>( new File( indexDirectory, "localDateTime" ), new LocalDateTimeLayout(), ValueGroup.LOCAL_DATE_TIME );
			  this._zonedDateTime = new FileLayout<ZonedDateTimeIndexKey>( new File( indexDirectory, "zonedDateTime" ), new ZonedDateTimeLayout(), ValueGroup.ZONED_DATE_TIME );
			  this._duration = new FileLayout<DurationIndexKey>( new File( indexDirectory, "duration" ), new DurationLayout(), ValueGroup.DURATION );
		 }

		 internal virtual IEnumerable<FileLayout> Existing()
		 {
			  IList<FileLayout> existing = new List<FileLayout>();
			  AddIfExists( existing, _date );
			  AddIfExists( existing, _localDateTime );
			  AddIfExists( existing, _zonedDateTime );
			  AddIfExists( existing, _localTime );
			  AddIfExists( existing, _zonedTime );
			  AddIfExists( existing, _duration );
			  return existing;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <T> void loadExistingIndexes(TemporalIndexCache<T> indexCache) throws java.io.IOException
		 internal virtual void LoadExistingIndexes<T>( TemporalIndexCache<T> indexCache )
		 {
			  foreach ( FileLayout fileLayout in Existing() )
			  {
					indexCache.Select( fileLayout.valueGroup );
			  }
		 }

		 internal virtual FileLayout<DateIndexKey> Date()
		 {
			  return _date;
		 }

		 internal virtual FileLayout<LocalTimeIndexKey> LocalTime()
		 {
			  return _localTime;
		 }

		 internal virtual FileLayout<ZonedTimeIndexKey> ZonedTime()
		 {
			  return _zonedTime;
		 }

		 internal virtual FileLayout<LocalDateTimeIndexKey> LocalDateTime()
		 {
			  return _localDateTime;
		 }

		 internal virtual FileLayout<ZonedDateTimeIndexKey> ZonedDateTime()
		 {
			  return _zonedDateTime;
		 }

		 internal virtual FileLayout<DurationIndexKey> Duration()
		 {
			  return _duration;
		 }

		 private void AddIfExists( IList<FileLayout> existing, FileLayout fileLayout )
		 {
			  if ( Exists( fileLayout ) )
			  {
					existing.Add( fileLayout );
			  }
		 }

		 private bool Exists( FileLayout fileLayout )
		 {
			  return fileLayout != null && _fs.fileExists( fileLayout.indexFile );
		 }

		 // .... we will add more explicit accessor methods later

		 internal class FileLayout<KEY> where KEY : NativeIndexSingleValueKey<KEY>
		 {
			  internal readonly File IndexFile;
			  internal readonly IndexLayout<KEY, NativeIndexValue> Layout;
			  internal readonly ValueGroup ValueGroup;

			  internal FileLayout( File indexFile, IndexLayout<KEY, NativeIndexValue> layout, ValueGroup valueGroup )
			  {
					this.IndexFile = indexFile;
					this.Layout = layout;
					this.ValueGroup = valueGroup;
			  }
		 }
	}

}