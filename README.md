# Media Organiser

Media Organiser is a Windows desktop application designed to help you organize your media files (images, videos, and documents) by providing an intuitive interface to browse, preview, categorize, and sort files into appropriate folders.

## Features

- **Media Preview**: View images, watch videos, and read documents directly within the application
- **File Organization**: Quickly categorize files as "Keep" or "Bin" with a simple interface
- **Smart Sorting**: Automatically organize files into folders by media type and optionally by year
- **Parent Folder Structure**: Option to maintain parent folder structure when organizing files
- **Image Rotation**: Rotate images for proper orientation before organizing
- **Session Management**: Automatically saves your progress and allows resuming previous sessions
- **Copy Mode**: Option to copy files instead of moving them, preserving the originals

## Supported File Types

- **Images**: JPG, JPEG, PNG, GIF, BMP, TIFF, WEBP
- **Videos**: MP4, AVI, MOV, WMV, MKV, FLV, WEBM
- **Documents**: PDF, DOCX, DOC, TXT, RTF

## System Requirements

- Windows 10 or later
- .NET 9.0 or higher
- Sufficient disk space for file operations

## Getting Started

1. **Select a Source Folder**: Click "Browse Folder" to select the directory containing your media files
2. **Scan Files**: Click "Scan Files" to analyze the directory and identify media files
3. **Review Files**: Navigate through the files using the "Previous" and "Next" buttons
4. **Categorize Files**: Mark each file as "Keep" or "Bin" using the respective buttons
5. **Organize Files**: Click "Organize Files" when you're ready to sort your files
6. **Select Destination**: Choose where your organized files will be saved

## Configuration Options

- **Copy Only**: When enabled, the original files will not be deleted
- **Sort By Year**: Organizes files into subdirectories by year
- **Keep Parent Folder**: Preserves the parent folder structure when organizing files

## Technical Implementation

Media Organiser is built using:
- C# with .NET 9.0
- Windows Forms for UI
- LanguageExt for functional programming patterns
- IronPDF for document rendering
- WPF MediaElement for video playback

The application follows functional programming principles using the LanguageExt library, with a focus on immutable data structures and error handling.

## Development

The codebase is organized into several key components:
- Core types and functional abstractions
- Media service for file operations
- Theme manager for consistent UI styling
- Document viewer with PDF rendering capabilities
- Video player with media controls
- State management with automatic serialization

## License

Media Organiser is licensed under the MIT License.

## Acknowledgments

- IronPDF for PDF rendering capabilities
- LanguageExt for functional programming extensions
