﻿using System;

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
namespace Org.Neo4j.Kernel.impl.security
{

	using Configuration = Org.Neo4j.Graphdb.config.Configuration;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using URLAccessRule = Org.Neo4j.Graphdb.security.URLAccessRule;
	using URLAccessValidationError = Org.Neo4j.Graphdb.security.URLAccessValidationError;

	internal class FileURLAccessRule : URLAccessRule
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URL validate(org.neo4j.graphdb.config.Configuration config, java.net.URL url) throws org.neo4j.graphdb.security.URLAccessValidationError
		 public override URL Validate( Configuration config, URL url )
		 {
			  if ( !( url.Authority == null || url.Authority.Equals( "" ) ) )
			  {
					throw new URLAccessValidationError( "file URL may not contain an authority section (i.e. it should be 'file:///')" );
			  }

			  if ( !( url.Query == null || url.Query.Equals( "" ) ) )
			  {
					throw new URLAccessValidationError( "file URL may not contain a query component" );
			  }

			  if ( !config.Get( GraphDatabaseSettings.allow_file_urls ) )
			  {
					throw new URLAccessValidationError( "configuration property '" + GraphDatabaseSettings.allow_file_urls.name() + "' is false" );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File root = config.get(org.neo4j.graphdb.factory.GraphDatabaseSettings.load_csv_file_url_root);
			  File root = config.Get( GraphDatabaseSettings.load_csv_file_url_root );
			  if ( root == null )
			  {
					return url;
			  }

			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.file.Path urlPath = java.nio.file.Paths.get(url.toURI());
					Path urlPath = Paths.get( url.toURI() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.file.Path rootPath = root.toPath().normalize().toAbsolutePath();
					Path rootPath = root.toPath().normalize().toAbsolutePath();
					// Normalize to prevent dirty tricks like '..'
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.file.Path result = rootPath.resolve(urlPath.getRoot().relativize(urlPath)).normalize().toAbsolutePath();
					Path result = rootPath.resolve( urlPath.Root.relativize( urlPath ) ).normalize().toAbsolutePath();

					if ( result.startsWith( rootPath ) )
					{
						 return result.toUri().toURL();
					}
					throw new URLAccessValidationError( "file URL points outside configured import directory" );
			  }
			  catch ( Exception e ) when ( e is MalformedURLException || e is URISyntaxException )
			  {
					// unreachable
					throw new Exception( e );
			  }
		 }
	}

}