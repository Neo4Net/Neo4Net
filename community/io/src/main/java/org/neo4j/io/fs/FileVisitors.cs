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
namespace Org.Neo4j.Io.fs
{

	using Org.Neo4j.Function;

	public class FileVisitors
	{
		 private FileVisitors()
		 {
		 }

		 public static FileVisitor<Path> OnlyMatching( System.Predicate<Path> predicate, FileVisitor<Path> wrapped )
		 {
			  return new FileVisitorAnonymousInnerClass( predicate, wrapped );
		 }

		 private class FileVisitorAnonymousInnerClass : FileVisitor<Path>
		 {
			 private System.Predicate<Path> _predicate;
			 private FileVisitor<Path> _wrapped;

			 public FileVisitorAnonymousInnerClass( System.Predicate<Path> predicate, FileVisitor<Path> wrapped )
			 {
				 this._predicate = predicate;
				 this._wrapped = wrapped;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult preVisitDirectory(java.nio.file.Path dir, java.nio.file.attribute.BasicFileAttributes attrs) throws java.io.IOException
			 public override FileVisitResult preVisitDirectory( Path dir, BasicFileAttributes attrs )
			 {
				  return _predicate( dir ) ? _wrapped.preVisitDirectory( dir, attrs ) : FileVisitResult.SKIP_SUBTREE;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult visitFile(java.nio.file.Path file, java.nio.file.attribute.BasicFileAttributes attrs) throws java.io.IOException
			 public override FileVisitResult visitFile( Path file, BasicFileAttributes attrs )
			 {
				  return _predicate( file ) ? _wrapped.visitFile( file, attrs ) : FileVisitResult.CONTINUE;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult visitFileFailed(java.nio.file.Path file, java.io.IOException e) throws java.io.IOException
			 public override FileVisitResult visitFileFailed( Path file, IOException e )
			 {
				  return _predicate( file ) ? _wrapped.visitFileFailed( file, e ) : FileVisitResult.CONTINUE;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult postVisitDirectory(java.nio.file.Path dir, java.io.IOException e) throws java.io.IOException
			 public override FileVisitResult postVisitDirectory( Path dir, IOException e )
			 {
				  return _predicate( dir ) ? _wrapped.postVisitDirectory( dir, e ) : FileVisitResult.CONTINUE;
			 }
		 }

		 public static FileVisitor<Path> ThrowExceptions( FileVisitor<Path> wrapped )
		 {
			  return new DecoratorAnonymousInnerClass( wrapped );
		 }

		 private class DecoratorAnonymousInnerClass : Decorator<Path>
		 {
			 public DecoratorAnonymousInnerClass( FileVisitor<Path> wrapped ) : base( wrapped )
			 {
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult visitFileFailed(java.nio.file.Path file, java.io.IOException e) throws java.io.IOException
			 public override FileVisitResult visitFileFailed( Path file, IOException e )
			 {
				  if ( e != null )
				  {
						throw e;
				  }
				  return base.visitFileFailed( file, null );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult postVisitDirectory(java.nio.file.Path dir, java.io.IOException e) throws java.io.IOException
			 public override FileVisitResult postVisitDirectory( Path dir, IOException e )
			 {
				  if ( e != null )
				  {
						throw e;
				  }
				  return base.postVisitDirectory( dir, null );
			 }
		 }

		 public static FileVisitor<Path> OnDirectory( ThrowingConsumer<Path, IOException> operation, FileVisitor<Path> wrapped )
		 {
			  return new DecoratorAnonymousInnerClass2( wrapped, operation );
		 }

		 private class DecoratorAnonymousInnerClass2 : Decorator<Path>
		 {
			 private ThrowingConsumer<Path, IOException> _operation;

			 public DecoratorAnonymousInnerClass2( FileVisitor<Path> wrapped, ThrowingConsumer<Path, IOException> operation ) : base( wrapped )
			 {
				 this._operation = operation;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult preVisitDirectory(java.nio.file.Path dir, java.nio.file.attribute.BasicFileAttributes attrs) throws java.io.IOException
			 public override FileVisitResult preVisitDirectory( Path dir, BasicFileAttributes attrs )
			 {
				  _operation.accept( dir );
				  return base.preVisitDirectory( dir, attrs );
			 }
		 }

		 public static FileVisitor<Path> OnFile( ThrowingConsumer<Path, IOException> operation, FileVisitor<Path> wrapped )
		 {
			  return new DecoratorAnonymousInnerClass3( wrapped, operation );
		 }

		 private class DecoratorAnonymousInnerClass3 : Decorator<Path>
		 {
			 private ThrowingConsumer<Path, IOException> _operation;

			 public DecoratorAnonymousInnerClass3( FileVisitor<Path> wrapped, ThrowingConsumer<Path, IOException> operation ) : base( wrapped )
			 {
				 this._operation = operation;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult visitFile(java.nio.file.Path file, java.nio.file.attribute.BasicFileAttributes attrs) throws java.io.IOException
			 public override FileVisitResult visitFile( Path file, BasicFileAttributes attrs )
			 {
				  _operation.accept( file );
				  return base.visitFile( file, attrs );
			 }
		 }

		 public static FileVisitor<Path> JustContinue()
		 {
			  return new FileVisitorAnonymousInnerClass2();
		 }

		 private class FileVisitorAnonymousInnerClass2 : FileVisitor<Path>
		 {
			 public override FileVisitResult preVisitDirectory( Path dir, BasicFileAttributes attrs )
			 {
				  return FileVisitResult.CONTINUE;
			 }

			 public override FileVisitResult visitFile( Path file, BasicFileAttributes attrs )
			 {
				  return FileVisitResult.CONTINUE;
			 }

			 public override FileVisitResult visitFileFailed( Path file, IOException e )
			 {
				  return FileVisitResult.CONTINUE;
			 }

			 public override FileVisitResult postVisitDirectory( Path dir, IOException e )
			 {
				  return FileVisitResult.CONTINUE;
			 }
		 }

		 public class Decorator<T> : FileVisitor<T>
		 {
			  internal readonly FileVisitor<T> Wrapped;

			  public Decorator( FileVisitor<T> wrapped )
			  {
					this.Wrapped = wrapped;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult preVisitDirectory(T t, java.nio.file.attribute.BasicFileAttributes attrs) throws java.io.IOException
			  public override FileVisitResult PreVisitDirectory( T t, BasicFileAttributes attrs )
			  {
					return Wrapped.preVisitDirectory( t, attrs );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult visitFile(T t, java.nio.file.attribute.BasicFileAttributes attrs) throws java.io.IOException
			  public override FileVisitResult VisitFile( T t, BasicFileAttributes attrs )
			  {
					return Wrapped.visitFile( t, attrs );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult visitFileFailed(T t, java.io.IOException e) throws java.io.IOException
			  public override FileVisitResult VisitFileFailed( T t, IOException e )
			  {
					return Wrapped.visitFileFailed( t, e );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.nio.file.FileVisitResult postVisitDirectory(T t, java.io.IOException e) throws java.io.IOException
			  public override FileVisitResult PostVisitDirectory( T t, IOException e )
			  {
					return Wrapped.postVisitDirectory( t, e );
			  }
		 }
	}

}