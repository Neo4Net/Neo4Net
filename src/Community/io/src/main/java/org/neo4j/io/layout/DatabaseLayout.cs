using System.Collections.Generic;

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
namespace Neo4Net.Io.layout
{

	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Streams = Neo4Net.Stream.Streams;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileUtils.getCanonicalFile;

	/// <summary>
	/// File layout representation of the particular database. Facade for any kind of file lookup for a particular database storage implementation.
	/// Any file retrieved from a layout can be considered a canonical file.
	/// <br/>
	/// No assumption should be made about where and how files of a particular database are positioned and all those details should be encapsulated inside.
	/// </summary>
	/// <seealso cref= StoreLayout </seealso>
	/// <seealso cref= DatabaseFile </seealso>
	public class DatabaseLayout
	{
		 private static readonly File[] _emptyFilesArray = new File[0];
		 private readonly File _databaseDirectory;
		 private readonly StoreLayout _storeLayout;
		 private readonly string _databaseName;

		 public static DatabaseLayout Of( StoreLayout storeLayout, string databaseName )
		 {
			  return new DatabaseLayout( storeLayout, databaseName );
		 }

		 public static DatabaseLayout Of( File databaseDirectory )
		 {
			  File canonicalFile = getCanonicalFile( databaseDirectory );
			  return Of( canonicalFile.ParentFile, canonicalFile.Name );
		 }

		 public static DatabaseLayout Of( File rootDirectory, string databaseName )
		 {
			  return new DatabaseLayout( StoreLayout.Of( rootDirectory ), databaseName );
		 }

		 private DatabaseLayout( StoreLayout storeLayout, string databaseName )
		 {
			  this._storeLayout = storeLayout;
			  this._databaseDirectory = new File( storeLayout.StoreDirectory(), databaseName );
			  this._databaseName = databaseName;
		 }

		 public virtual string DatabaseName
		 {
			 get
			 {
				  return _databaseName;
			 }
		 }

		 public virtual StoreLayout StoreLayout
		 {
			 get
			 {
				  return _storeLayout;
			 }
		 }

		 public virtual File DatabaseDirectory()
		 {
			  return _databaseDirectory;
		 }

		 public virtual File MetadataStore()
		 {
			  return file( DatabaseFile.MetadataStore.Name );
		 }

		 public virtual File LabelScanStore()
		 {
			  return file( DatabaseFile.LabelScanStore.Name );
		 }

		 public virtual File CountStoreA()
		 {
			  return file( DatabaseFile.CountsStoreA.Name );
		 }

		 public virtual File CountStoreB()
		 {
			  return file( DatabaseFile.CountsStoreB.Name );
		 }

		 public virtual File PropertyStringStore()
		 {
			  return file( DatabaseFile.PropertyStringStore.Name );
		 }

		 public virtual File RelationshipStore()
		 {
			  return file( DatabaseFile.RelationshipStore.Name );
		 }

		 public virtual File PropertyStore()
		 {
			  return file( DatabaseFile.PropertyStore.Name );
		 }

		 public virtual File NodeStore()
		 {
			  return file( DatabaseFile.NodeStore.Name );
		 }

		 public virtual File NodeLabelStore()
		 {
			  return file( DatabaseFile.NodeLabelStore.Name );
		 }

		 public virtual File PropertyArrayStore()
		 {
			  return file( DatabaseFile.PropertyArrayStore.Name );
		 }

		 public virtual File PropertyKeyTokenStore()
		 {
			  return file( DatabaseFile.PropertyKeyTokenStore.Name );
		 }

		 public virtual File PropertyKeyTokenNamesStore()
		 {
			  return file( DatabaseFile.PropertyKeyTokenNamesStore.Name );
		 }

		 public virtual File RelationshipTypeTokenStore()
		 {
			  return file( DatabaseFile.RelationshipTypeTokenStore.Name );
		 }

		 public virtual File RelationshipTypeTokenNamesStore()
		 {
			  return file( DatabaseFile.RelationshipTypeTokenNamesStore.Name );
		 }

		 public virtual File LabelTokenStore()
		 {
			  return file( DatabaseFile.LabelTokenStore.Name );
		 }

		 public virtual File SchemaStore()
		 {
			  return file( DatabaseFile.SchemaStore.Name );
		 }

		 public virtual File RelationshipGroupStore()
		 {
			  return file( DatabaseFile.RelationshipGroupStore.Name );
		 }

		 public virtual File LabelTokenNamesStore()
		 {
			  return file( DatabaseFile.LabelTokenNamesStore.Name );
		 }

		 public virtual ISet<File> IdFiles()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return java.util.DatabaseFile.values().Where(DatabaseFile::hasIdFile).flatMap(value => Streams.ofOptional(idFile(value))).collect(Collectors.toSet());
		 }

		 public virtual ISet<File> StoreFiles()
		 {
			  return Arrays.stream( DatabaseFile.values() ).flatMap(this.file).collect(Collectors.toSet());
		 }

		 public virtual Optional<File> IdFile( DatabaseFile file )
		 {
			  return file.hasIdFile() ? idFile(file.Name) : null;
		 }

		 public virtual File File( string fileName )
		 {
			  return new File( _databaseDirectory, fileName );
		 }

		 public virtual Stream<File> File( DatabaseFile databaseFile )
		 {
			  IEnumerable<string> names = databaseFile.Names;
			  return Iterables.stream( names ).map( this.file );
		 }

		 public virtual File[] ListDatabaseFiles( FilenameFilter filter )
		 {
			  File[] files = _databaseDirectory.listFiles( filter );
			  return files != null ? files : _emptyFilesArray;
		 }

		 public virtual File IdMetadataStore()
		 {
			  return idFile( DatabaseFile.MetadataStore.Name );
		 }

		 public virtual File IdNodeStore()
		 {
			  return idFile( DatabaseFile.NodeStore.Name );
		 }

		 public virtual File IdNodeLabelStore()
		 {
			  return idFile( DatabaseFile.NodeLabelStore.Name );
		 }

		 public virtual File IdPropertyStore()
		 {
			  return idFile( DatabaseFile.PropertyStore.Name );
		 }

		 public virtual File IdPropertyKeyTokenStore()
		 {
			  return idFile( DatabaseFile.PropertyKeyTokenStore.Name );
		 }

		 public virtual File IdPropertyKeyTokenNamesStore()
		 {
			  return idFile( DatabaseFile.PropertyKeyTokenNamesStore.Name );
		 }

		 public virtual File IdPropertyStringStore()
		 {
			  return idFile( DatabaseFile.PropertyStringStore.Name );
		 }

		 public virtual File IdPropertyArrayStore()
		 {
			  return idFile( DatabaseFile.PropertyArrayStore.Name );
		 }

		 public virtual File IdRelationshipStore()
		 {
			  return idFile( DatabaseFile.RelationshipStore.Name );
		 }

		 public virtual File IdRelationshipGroupStore()
		 {
			  return idFile( DatabaseFile.RelationshipGroupStore.Name );
		 }

		 public virtual File IdRelationshipTypeTokenStore()
		 {
			  return idFile( DatabaseFile.RelationshipTypeTokenStore.Name );
		 }

		 public virtual File IdRelationshipTypeTokenNamesStore()
		 {
			  return idFile( DatabaseFile.RelationshipTypeTokenNamesStore.Name );
		 }

		 public virtual File IdLabelTokenStore()
		 {
			  return idFile( DatabaseFile.LabelTokenStore.Name );
		 }

		 public virtual File IdLabelTokenNamesStore()
		 {
			  return idFile( DatabaseFile.LabelTokenNamesStore.Name );
		 }

		 public virtual File IdSchemaStore()
		 {
			  return idFile( DatabaseFile.SchemaStore.Name );
		 }

		 private File IdFile( string name )
		 {
			  return File( IdFileName( name ) );
		 }

		 private static string IdFileName( string storeName )
		 {
			  return storeName + ".id";
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _databaseDirectory, _storeLayout );
		 }

		 public override string ToString()
		 {
			  return _databaseDirectory.ToString();
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  DatabaseLayout that = ( DatabaseLayout ) o;
			  return Objects.Equals( _databaseDirectory, that._databaseDirectory ) && Objects.Equals( _storeLayout, that._storeLayout );
		 }
	}

}